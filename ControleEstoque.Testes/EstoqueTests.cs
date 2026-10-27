using System;
using System.IO;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;
using Microsoft.Data.Sqlite;
using Xunit;

namespace ControleEstoque.Testes;

// Cada teste ganha um banco SQLite novo em arquivo temporário (nunca toca no estoque.db real)
// e o seu próprio "usuário logado". Não há estado compartilhado entre os testes.
public class EstoqueTests : IDisposable
{
    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"estoque-teste-{Guid.NewGuid():N}.db");
    private readonly ContextoDoTenantDesktop _tenant = new();
    private readonly ProdutoService _produtos;
    private readonly MovimentacaoService _movs;
    private readonly int _empresaA;
    private readonly int _empresaB;

    public EstoqueTests()
    {
        var fabrica = new AppDbContextFactory(_tenant, _arquivo);
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
        SqliteConnection.ClearAllPools();
        try { File.Delete(_arquivo); } catch (IOException) { }
    }

    private static Empresa NovaEmpresa(string nome) => new()
    {
        NomeEmpresa = nome,
        StatusAssinatura = StatusAssinatura.Ativa,
        DataVencimento = DateTime.Now.AddYears(1)
    };

    private void EntrarComo(int empresaId) =>
        _tenant.Iniciar(new Usuario { Id = 1, EmpresaId = empresaId });

    private int CriarProduto(string codigo, int quantidade)
    {
        var produto = new Produto
        {
            Codigo = codigo,
            Nome = $"Produto {codigo}",
            Quantidade = quantidade,
            PrecoCusto = 1m,
            PrecoVenda = 2m
        };
        _produtos.Add(produto);
        return produto.Id;
    }

    [Fact]
    public void Saida_maior_que_o_estoque_lanca_excecao_e_nao_altera_nada()
    {
        var id = CriarProduto("P1", 5);

        Assert.Throws<EstoqueInsuficienteException>(() =>
            _movs.RegistrarSaida(id, 10, "Venda", DateTime.Now));

        Assert.Equal(5, _produtos.GetById(id)!.Quantidade);
        Assert.Empty(_movs.GetHistorico());
    }

    [Fact]
    public void Saida_valida_diminui_o_estoque_e_registra_a_movimentacao()
    {
        var id = CriarProduto("P1", 10);

        _movs.RegistrarSaida(id, 6, "Venda", DateTime.Now);

        Assert.Equal(4, _produtos.GetById(id)!.Quantidade);
        Assert.Single(_movs.GetHistorico());
    }

    [Fact]
    public void Saida_do_estoque_inteiro_e_permitida()
    {
        var id = CriarProduto("P1", 3);

        _movs.RegistrarSaida(id, 3, null, DateTime.Now);

        Assert.Equal(0, _produtos.GetById(id)!.Quantidade);
    }

    [Fact]
    public void Entrada_soma_ao_estoque_e_atualiza_o_custo()
    {
        var id = CriarProduto("P1", 10);

        _movs.RegistrarEntrada(id, 15, 3.50m, "Fornecedor X", DateTime.Now);

        var produto = _produtos.GetById(id)!;
        Assert.Equal(25, produto.Quantidade);
        Assert.Equal(3.50m, produto.PrecoCusto);
        Assert.Single(_movs.GetHistorico());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void Quantidade_zero_ou_negativa_e_recusada(int quantidade)
    {
        var id = CriarProduto("P1", 10);

        Assert.Throws<ArgumentException>(() => _movs.RegistrarEntrada(id, quantidade, null, null, DateTime.Now));
        Assert.Throws<ArgumentException>(() => _movs.RegistrarSaida(id, quantidade, null, DateTime.Now));
    }

    [Fact]
    public void Produto_inativo_nao_recebe_movimentacao()
    {
        var id = CriarProduto("P1", 10);
        _produtos.Delete(id); // na prática só inativa

        Assert.Throws<InvalidOperationException>(() => _movs.RegistrarEntrada(id, 1, null, null, DateTime.Now));
        Assert.Throws<InvalidOperationException>(() => _movs.RegistrarSaida(id, 1, null, DateTime.Now));
    }

    [Fact]
    public void Uma_empresa_nao_enxerga_nem_movimenta_o_produto_da_outra()
    {
        var idDaA = CriarProduto("ISO", 5);

        EntrarComo(_empresaB);

        Assert.Null(_produtos.GetById(idDaA));
        Assert.Empty(_produtos.GetAll());
        Assert.Throws<InvalidOperationException>(() => _movs.RegistrarEntrada(idDaA, 1, null, null, DateTime.Now));
        Assert.Throws<InvalidOperationException>(() => _movs.RegistrarSaida(idDaA, 1, null, DateTime.Now));

        // o mesmo código de produto pode existir nas duas empresas
        Assert.True(CriarProduto("ISO", 1) > 0);

        EntrarComo(_empresaA);
        Assert.Equal(5, _produtos.GetById(idDaA)!.Quantidade);
    }

    // Garante que editar um produto não desfaz movimentações feitas enquanto a tela estava aberta.
    [Fact]
    public void Editar_produto_nao_pode_desfazer_movimentacao_feita_no_meio_tempo()
    {
        var id = CriarProduto("P1", 5);

        var copiaDaTela = _produtos.GetById(id)!;
        _movs.RegistrarSaida(id, 2, "Venda", DateTime.Now);
        copiaDaTela.Nome = "Novo nome";
        _produtos.Update(copiaDaTela);

        Assert.Equal(3, _produtos.GetById(id)!.Quantidade);
    }

    [Fact]
    public void Codigo_repetido_na_mesma_empresa_e_recusado_sem_diferenciar_maiusculas()
    {
        CriarProduto("abc", 1);

        var duplicado = new Produto { Codigo = "ABC", Nome = "Outro", PrecoCusto = 1m, PrecoVenda = 2m };

        Assert.Throws<InvalidOperationException>(() => _produtos.Add(duplicado));
        Assert.Single(_produtos.GetAll());
    }

    [Fact]
    public void Codigo_de_barras_repetido_e_recusado()
    {
        _produtos.Add(new Produto { Codigo = "A", Nome = "A", CodigoBarras = "789", PrecoCusto = 1m, PrecoVenda = 2m });

        var outro = new Produto { Codigo = "B", Nome = "B", CodigoBarras = "789", PrecoCusto = 1m, PrecoVenda = 2m };

        Assert.Throws<InvalidOperationException>(() => _produtos.Add(outro));
    }

    [Fact]
    public void Editar_produto_para_o_codigo_de_outro_e_recusado()
    {
        CriarProduto("P1", 1);
        var idP2 = CriarProduto("P2", 1);

        var p2 = _produtos.GetById(idP2)!;
        p2.Codigo = "P1";

        Assert.Throws<InvalidOperationException>(() => _produtos.Update(p2));
    }

    [Fact]
    public void Produto_com_preco_negativo_e_recusado()
    {
        var invalido = new Produto { Codigo = "N1", Nome = "Neg", PrecoCusto = -1m, PrecoVenda = 2m };

        Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(() => _produtos.Add(invalido));
    }

    [Fact]
    public void Contexto_do_tenant_comeca_vazio_e_volta_a_ficar_vazio_depois_do_logout()
    {
        var tenant = new ContextoDoTenantDesktop();
        Assert.Equal(0, tenant.EmpresaId);

        tenant.Iniciar(new Usuario { Id = 7, EmpresaId = 3 });
        Assert.Equal(3, tenant.EmpresaId);
        Assert.Equal(7, tenant.UsuarioId);

        tenant.Encerrar();
        Assert.Equal(0, tenant.EmpresaId);
        Assert.Equal(0, tenant.UsuarioId);
    }

    [Fact]
    public void Sem_empresa_na_sessao_nao_e_possivel_gravar_produto()
    {
        _tenant.Encerrar();

        var produto = new Produto { Codigo = "X", Nome = "X", PrecoCusto = 1m, PrecoVenda = 2m };

        Assert.Throws<InvalidOperationException>(() => _produtos.Add(produto));
    }

    [Fact]
    public void Dois_contextos_com_tenants_diferentes_no_mesmo_banco_nao_se_misturam()
    {
        CriarProduto("A1", 1);

        var outroTenant = new ContextoDoTenantDesktop();
        outroTenant.Iniciar(new Usuario { Id = 2, EmpresaId = _empresaB });
        var produtosDaB = new ProdutoService(new AppDbContextFactory(outroTenant, _arquivo));

        Assert.Empty(produtosDaB.GetAll());
        Assert.Single(_produtos.GetAll());
    }
}
