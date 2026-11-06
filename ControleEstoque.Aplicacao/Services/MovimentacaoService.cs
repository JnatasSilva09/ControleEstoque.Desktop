using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Dominio.Models;
using ControleEstoque.Infra.Data;

namespace ControleEstoque.Aplicacao.Services
{
    public class RegraDeNegocioException : Exception
    {
        public RegraDeNegocioException(string mensagem) : base(mensagem) { }
    }

    public class EstoqueInsuficienteException : RegraDeNegocioException
    {
        public EstoqueInsuficienteException(string mensagem) : base(mensagem) { }
    }

    public class MovimentacaoService
    {
        private readonly IAppDbContextFactory _fabrica;

        public MovimentacaoService(IAppDbContextFactory fabrica)
        {
            _fabrica = fabrica;
        }

        public async Task RegistrarEntradaAsync(int produtoId, int quantidade, decimal? valorCusto, string? fornecedorNome, DateTime data, CancellationToken ct = default)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade de entrada deve ser maior que zero.");

            await using var db = _fabrica.Criar();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var produto = await db.Produtos.FirstOrDefaultAsync(p => p.Id == produtoId, ct)
                    ?? throw new RegraDeNegocioException("Produto não encontrado.");

                if (!produto.Ativo)
                    throw new RegraDeNegocioException("Este produto está inativo e não pode receber movimentações.");

                produto.Quantidade += quantidade;

                if (valorCusto.HasValue && valorCusto.Value > 0)
                {
                    produto.PrecoCusto = valorCusto.Value;
                }

                var movimentacao = new Movimentacao
                {
                    ProdutoId = produtoId,
                    Tipo = TipoMovimentacao.Entrada,
                    Quantidade = quantidade,
                    ValorCusto = valorCusto,
                    FornecedorNome = fornecedorNome?.Trim(),
                    Data = data
                };

                db.Movimentacoes.Add(movimentacao);
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task RegistrarSaidaAsync(int produtoId, int quantidade, string? motivo, DateTime data, CancellationToken ct = default)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade de saída deve ser maior que zero.");

            await using var db = _fabrica.Criar();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                // Subtração atômica no banco: previne condição de corrida (dois usuários vendendo o último item juntos)
                var linhasAfetadas = await db.Produtos
                    .Where(p => p.Id == produtoId && p.Ativo && p.Quantidade >= quantidade)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantidade, p => p.Quantidade - quantidade), ct);

                if (linhasAfetadas == 0)
                {
                    var p = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == produtoId, ct);
                    if (p is null)
                        throw new RegraDeNegocioException("Produto não encontrado.");
                    if (!p.Ativo)
                        throw new RegraDeNegocioException("Este produto está inativo.");

                    throw new EstoqueInsuficienteException(
                        $"Não é possível registrar a saída. Estoque atual de \"{p.Nome}\": {p.Quantidade} unidade(s). " +
                        $"Quantidade solicitada: {quantidade} unidade(s).");
                }

                var movimentacao = new Movimentacao
                {
                    ProdutoId = produtoId,
                    Tipo = TipoMovimentacao.Saida,
                    Quantidade = quantidade,
                    Motivo = motivo?.Trim(),
                    Data = data
                };

                db.Movimentacoes.Add(movimentacao);
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<List<Movimentacao>> GetHistoricoAsync(string? produtoFiltro = null, DateTime? inicio = null, DateTime? fim = null, CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            var query = db.Movimentacoes
                .Include(m => m.Produto)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(produtoFiltro))
            {
                var f = produtoFiltro.Trim();
                query = query.Where(m => m.Produto != null && EF.Functions.ILike(m.Produto.Nome, $"%{f}%"));
            }

            if (inicio.HasValue)
            {
                var dataInicio = inicio.Value.Date;
                query = query.Where(m => m.Data >= dataInicio);
            }

            if (fim.HasValue)
            {
                var dataFim = fim.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.Data <= dataFim);
            }

            return await query.OrderByDescending(m => m.Data).ToListAsync(ct);
        }

        public async Task<int> TotalEntradasAsync(CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            return await db.Movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .SumAsync(m => (int?)m.Quantidade, ct) ?? 0;
        }

        public async Task<int> TotalSaidasAsync(CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            return await db.Movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Saida)
                .SumAsync(m => (int?)m.Quantidade, ct) ?? 0;
        }

        public async Task<List<Movimentacao>> ObterMovimentacoesAsync(CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            return await db.Movimentacoes.ToListAsync(ct);
        }
    }
}