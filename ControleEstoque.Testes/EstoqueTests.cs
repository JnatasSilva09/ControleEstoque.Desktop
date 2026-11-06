using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Dominio.Contexto;
using ControleEstoque.Dominio.Models;
using ControleEstoque.Infra.Data;

namespace ControleEstoque.Testes;

public class EstoqueTests : IDisposable
{
    private readonly string _conexao = BancoDeTestes.NovaConexao();
    private readonly ContextoDoTenantManual _tenant = new();
    private readonly ProdutoService _produtos;
    private readonly MovimentacaoService _movs;
    private readonly int _empresaA;
    private readonly int _empresaB;

    public EstoqueTests()
    {
        var fabrica = new AppDbContextFactory(_tenant, _conexao);
        _produtos = new ProdutoService(fabrica);
        _movs = new MovimentacaoService(fabrica);

        using var db = fabrica.Criar();
        db.Database.EnsureCreated();

        var a = NovaEmpresa("Empresa A");
        var b = NovaEmpresa("Empresa B");
        db.Empresas.AddRange(a, b);
        db.SaveChanges();

        _empresaA = a.Id;
        _empresaB = b.Id;
        EntrarComo(_empresaA);
    }

    public void Dispose()
    {
        _tenant.Encerrar();
        using (var db = new AppDbContextFactory(_tenant, _conexao).Criar())
        {
            db.Database.EnsureDeleted();
        }
        Npgsql.NpgsqlConnection.ClearAllPools();
    }

    private static Empresa NovaEmpresa(string nome) => new()
    {
        NomeEmpresa = nome,
        StatusAssinatura = StatusAssinatura.Ativa,
        DataVencimento = DateTime.Now.AddYears(1)
    };

    private void EntrarComo(int empresaId) =>
        _tenant.Iniciar(new Usuario { Id = 1, EmpresaId = empresaId });

    private async Task<int> CriarProdutoAsync(string codigo, int quantidade)
    {
        var produto = new Produto
        {
            Codigo = codigo,
            Nome = $"Produto {codigo}",
            Quantidade = quantidade,
            PrecoCusto = 1m,
            PrecoVenda = 2m
        };
        await _produtos.AddAsync(produto);
        return produto.Id;
    }

    [Fact]
    public async Task Saida_maior_que_o_estoque_lanca_excecao_e_nao_altera_nada()
    {
        var id = await CriarProdutoAsync("P1", 5);

        await Assert.ThrowsAsync<EstoqueInsuficienteException>(() =>
            _movs.RegistrarSaidaAsync(id, 10, "Venda", DateTime.Now));

        var prod = await _produtos.GetByIdAsync(id);
        Assert.Equal(5, prod!.Quantidade);
        var historico = await _movs.GetHistoricoAsync();
        Assert.Empty(historico);
    }

    [Fact]
    public async Task Saida_valida_diminui_o_estoque_e_registra_a_movimentacao()
    {
        var id = await CriarProdutoAsync("P1", 10);

        await _movs.RegistrarSaidaAsync(id, 6, "Venda", DateTime.Now);

        var prod = await _produtos.GetByIdAsync(id);
        Assert.Equal(4, prod!.Quantidade);
        var historico = await _movs.GetHistoricoAsync();
        Assert.Single(historico);
    }

    [Fact]
    public async Task Saida_do_estoque_inteiro_e_permitida()
    {
        var id = await CriarProdutoAsync("P1", 3);

        await _movs.RegistrarSaidaAsync(id, 3, null, DateTime.Now);

        var prod = await _produtos.GetByIdAsync(id);
        Assert.Equal(0, prod!.Quantidade);
    }

    [Fact]
    public async Task Entrada_soma_ao_estoque_e_atualiza_o_custo()
    {
        var id = await CriarProdutoAsync("P1", 10);

        await _movs.RegistrarEntradaAsync(id, 15, 3.50m, "Fornecedor X", DateTime.Now);

        var produto = await _produtos.GetByIdAsync(id);
        Assert.NotNull(produto);
        Assert.Equal(25, produto.Quantidade);
        Assert.Equal(3.50m, produto.PrecoCusto);
        var historico = await _movs.GetHistoricoAsync();
        Assert.Single(historico);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public async Task Quantidade_zero_ou_negativa_e_recusada(int quantidade)
    {
        var id = await CriarProdutoAsync("P1", 10);

        await Assert.ThrowsAsync<ArgumentException>(() => _movs.RegistrarEntradaAsync(id, quantidade, null, null, DateTime.Now));
        await Assert.ThrowsAsync<ArgumentException>(() => _movs.RegistrarSaidaAsync(id, quantidade, null, DateTime.Now));
    }

[Fact]
public async Task Produto_inativo_nao_recebe_movimentacao()
{
    var id = await CriarProdutoAsync("P1", 10);
    await _produtos.DeleteAsync(id);

    await Assert.ThrowsAsync<RegraDeNegocioException>(() => _movs.RegistrarEntradaAsync(id, 1, null, null, DateTime.Now));
    await Assert.ThrowsAsync<RegraDeNegocioException>(() => _movs.RegistrarSaidaAsync(id, 1, null, DateTime.Now));
}
    [Fact]
    public async Task Uma_empresa_nao_enxerga_nem_movimenta_o_produto_da_outra()
    {
        var idDaA = await CriarProdutoAsync("ISO", 5);

        EntrarComo(_empresaB);

        Assert.Null(await _produtos.GetByIdAsync(idDaA));
        var todosB = await _produtos.GetAllAsync();
        Assert.Empty(todosB);
     await Assert.ThrowsAsync<RegraDeNegocioException>(() => _movs.RegistrarEntradaAsync(idDaA, 1, null, null, DateTime.Now));
await Assert.ThrowsAsync<RegraDeNegocioException>(() => _movs.RegistrarSaidaAsync(idDaA, 1, null, DateTime.Now));
        var novoIdB = await CriarProdutoAsync("ISO", 1);
        Assert.True(novoIdB > 0);

        EntrarComo(_empresaA);
        var prodA = await _produtos.GetByIdAsync(idDaA);
        Assert.Equal(5, prodA!.Quantidade);
    }

    [Fact]
    public async Task Editar_produto_nao_pode_desfazer_movimentacao_feita_no_meio_tempo()
    {
        var id = await CriarProdutoAsync("P1", 5);

        var copiaDaTela = await _produtos.GetByIdAsync(id);
        await _movs.RegistrarSaidaAsync(id, 2, "Venda", DateTime.Now);
        copiaDaTela!.Nome = "Novo nome";
        await _produtos.UpdateAsync(copiaDaTela);

        var prodAtual = await _produtos.GetByIdAsync(id);
        Assert.Equal(3, prodAtual!.Quantidade);
    }

    [Fact]
    public async Task Codigo_repetido_na_mesma_empresa_e_recusado_sem_diferenciar_maiusculas()
    {
        await _produtos.AddAsync(new Produto { Codigo = "abc", Nome = "A", PrecoCusto = 1m, PrecoVenda = 2m });

        var duplicado = new Produto { Codigo = "ABC", Nome = "Outro", PrecoCusto = 1m, PrecoVenda = 2m };

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => _produtos.AddAsync(duplicado));
        var todos = await _produtos.GetAllAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task Codigo_de_barras_repetido_e_recusado()
    {
        await _produtos.AddAsync(new Produto { Codigo = "A", Nome = "A", CodigoBarras = "789", PrecoCusto = 1m, PrecoVenda = 2m });

        var outro = new Produto { Codigo = "B", Nome = "B", CodigoBarras = "789", PrecoCusto = 1m, PrecoVenda = 2m };

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => _produtos.AddAsync(outro));
    }

    [Fact]
    public async Task Editar_produto_para_o_codigo_de_outro_e_recusado()
    {
        await CriarProdutoAsync("P1", 1);
        var idP2 = await CriarProdutoAsync("P2", 1);

        var p2 = await _produtos.GetByIdAsync(idP2);
        p2!.Codigo = "P1";

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => _produtos.UpdateAsync(p2));
    }

    [Fact]
    public async Task Produto_com_preco_negativo_e_recusado()
    {
        var invalido = new Produto { Codigo = "N1", Nome = "Neg", PrecoCusto = -1m, PrecoVenda = 2m };

        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => _produtos.AddAsync(invalido));
    }

    [Fact]
    public void Contexto_do_tenant_comeca_vazio_e_volta_a_ficar_vazio_depois_do_logout()
    {
        var tenant = new ContextoDoTenantManual();
        Assert.Equal(0, tenant.EmpresaId);

        tenant.Iniciar(new Usuario { Id = 7, EmpresaId = 3 });
        Assert.Equal(3, tenant.EmpresaId);
        Assert.Equal(7, tenant.UsuarioId);

        tenant.Encerrar();
        Assert.Equal(0, tenant.EmpresaId);
        Assert.Equal(0, tenant.UsuarioId);
    }

    [Fact]
    public async Task Sem_empresa_na_sessao_nao_e_possivel_gravar_produto()
    {
        _tenant.Encerrar();

        var produto = new Produto { Codigo = "X", Nome = "X", PrecoCusto = 1m, PrecoVenda = 2m };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _produtos.AddAsync(produto));
    }

    [Fact]
    public async Task Dois_contextos_com_tenants_diferentes_no_mesmo_banco_nao_se_misturam()
    {
        await CriarProdutoAsync("A1", 1);

        var outroTenant = new ContextoDoTenantManual();
        outroTenant.Iniciar(new Usuario { Id = 2, EmpresaId = _empresaB });
        var produtosDaB = new ProdutoService(new AppDbContextFactory(outroTenant, _conexao));

        var listaB = await produtosDaB.GetAllAsync();
        var listaA = await _produtos.GetAllAsync();

        Assert.Empty(listaB);
        Assert.Single(listaA);
    }

    [Fact]
    public async Task Saidas_simultaneas_nunca_deixam_estoque_negativo()
    {
        var id = await CriarProdutoAsync("CONC", 10);

        var tarefas = Enumerable.Range(0, 20).Select(async _ =>
        {
            try
            {
                await _movs.RegistrarSaidaAsync(id, 1, "Concorrência", DateTime.Now);
                return true;
            }
            catch (EstoqueInsuficienteException)
            {
                return false;
            }
        });

        var resultados = await Task.WhenAll(tarefas);

        Assert.Equal(10, resultados.Count(sucesso => sucesso));
        
        var produtoAtualizado = await _produtos.GetByIdAsync(id);
        Assert.NotNull(produtoAtualizado);
        Assert.Equal(0, produtoAtualizado.Quantidade);
    }
}