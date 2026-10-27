using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    // Exceção específica para quando o usuário tenta tirar mais estoque do que existe.
    public class EstoqueInsuficienteException : Exception
    {
        public EstoqueInsuficienteException(string mensagem) : base(mensagem) { }
    }

    public class MovimentacaoService
    {
        public void RegistrarEntrada(int produtoId, int quantidade, decimal? valorCusto, string? fornecedorNome, DateTime data)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade de entrada deve ser maior que zero.");

            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                var produto = db.Produtos.FirstOrDefault(p => p.Id == produtoId)
                    ?? throw new InvalidOperationException("Produto não encontrado.");

                if (!produto.Ativo)
                    throw new InvalidOperationException("Este produto está inativo e não pode receber movimentações.");

                // Regra: entrada sempre aumenta o estoque
                produto.Quantidade += quantidade;

                // Se um valor de custo foi informado, atualizamos o custo do produto
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
                db.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void RegistrarSaida(int produtoId, int quantidade, string? motivo, DateTime data)
        {
            if (quantidade <= 0)
                throw new ArgumentException("A quantidade de saída deve ser maior que zero.");

            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                var produto = db.Produtos.FirstOrDefault(p => p.Id == produtoId)
                    ?? throw new InvalidOperationException("Produto não encontrado.");

                if (!produto.Ativo)
                    throw new InvalidOperationException("Este produto está inativo e não pode receber movimentações.");

                // Regra principal do sistema: não pode sair mais do que existe em estoque
                if (quantidade > produto.Quantidade)
                {
                    throw new EstoqueInsuficienteException(
                        $"Não é possível registrar a saída. Estoque atual de \"{produto.Nome}\": {produto.Quantidade} unidade(s). " +
                        $"Quantidade solicitada: {quantidade} unidade(s).");
                }

                produto.Quantidade -= quantidade;

                var movimentacao = new Movimentacao
                {
                    ProdutoId = produtoId,
                    Tipo = TipoMovimentacao.Saida,
                    Quantidade = quantidade,
                    Motivo = motivo?.Trim(),
                    Data = data
                };

                db.Movimentacoes.Add(movimentacao);
                db.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Movimentacao> GetHistorico(string? produtoFiltro = null, DateTime? inicio = null, DateTime? fim = null)
        {
            using var db = new AppDbContext();
            var query = db.Movimentacoes
                .Include(m => m.Produto)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(produtoFiltro))
            {
                var f = produtoFiltro.Trim();
                query = query.Where(m => m.Produto != null && EF.Functions.Like(m.Produto.Nome, $"%{f}%"));
            }

            if (inicio.HasValue)
            {
                var dataInicio = inicio.Value.Date;
                query = query.Where(m => m.Data >= dataInicio);
            }

            if (fim.HasValue)
            {
                // Garante que a busca pegue até o último segundo do dia final (23:59:59)
                var dataFim = fim.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.Data <= dataFim);
            }

            return query.OrderByDescending(m => m.Data).ToList();
        }

        public int TotalEntradas()
        {
            using var db = new AppDbContext();
            return db.Movimentacoes
                .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                .Sum(m => (int?)m.Quantidade) ?? 0;
        }

       public int TotalSaidas()
        {
            using var db = new AppDbContext();
            return db.Movimentacoes
                     .Where(m => m.Tipo == TipoMovimentacao.Saida)
                     .Sum(m => (int?)m.Quantidade) ?? 0;
        }

        /// <summary>
        /// Retorna todas as movimentações registadas para efeitos de relatório.
        /// </summary>
        public List<Movimentacao> ObterMovimentacoes()
        {
            using var db = new AppDbContext();
            return db.Movimentacoes.ToList();
        }
    } // <-- Esta é a chave que fecha a classe MovimentacaoService
} // <-- Esta é a chave que fecha o namespace