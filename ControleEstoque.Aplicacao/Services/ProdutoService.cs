using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ControleEstoque.Aplicacao.Helpers;
using ControleEstoque.Dominio.Models;
using ControleEstoque.Infra.Data;

namespace ControleEstoque.Aplicacao.Services
{
    public class ProdutoService
    {
        private readonly IAppDbContextFactory _fabrica;

        public ProdutoService(IAppDbContextFactory fabrica)
        {
            _fabrica = fabrica;
        }

        public async Task<List<Produto>> GetAllAsync(string? filtro = null, CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            var query = db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .AsNoTracking()
                .Where(p => p.Ativo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var f = filtro.Trim();
                query = query.Where(p =>
                    EF.Functions.ILike(p.Nome, $"%{f}%") ||
                    EF.Functions.ILike(p.Codigo, $"%{f}%") ||
                    (p.CodigoBarras != null && EF.Functions.ILike(p.CodigoBarras, $"%{f}%")));
            }

            return await query.OrderBy(p => p.Nome).ToListAsync(ct);
        }

        public async Task<Produto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            return await db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task AddAsync(Produto produto, CancellationToken ct = default)
        {
            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
                produto.CodigoBarras = produto.CodigoBarras.Trim();

            ValidacaoHelper.Garantir(produto);
            produto.Ativo = true;

            await using var db = _fabrica.Criar();
            db.Produtos.Add(produto);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                throw new RegraDeNegocioException("Já existe um produto cadastrado com este código ou código de barras.");
            }
        }

        public async Task UpdateAsync(Produto produto, CancellationToken ct = default)
        {
            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
                produto.CodigoBarras = produto.CodigoBarras.Trim();

            ValidacaoHelper.Garantir(produto);

            await using var db = _fabrica.Criar();
            var atual = await db.Produtos.FirstOrDefaultAsync(p => p.Id == produto.Id, ct)
                ?? throw new RegraDeNegocioException("Produto não encontrado.");

            atual.Codigo = produto.Codigo;
            atual.CodigoBarras = produto.CodigoBarras;
            atual.Nome = produto.Nome;
            atual.Descricao = produto.Descricao;
            atual.UnidadeMedida = produto.UnidadeMedida;
            atual.PrecoCusto = produto.PrecoCusto;
            atual.PrecoVenda = produto.PrecoVenda;
            atual.EstoqueMinimo = produto.EstoqueMinimo;
            atual.CategoriaId = produto.CategoriaId;
            atual.FornecedorId = produto.FornecedorId;

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                throw new RegraDeNegocioException("Já existe outro produto cadastrado com este código ou código de barras.");
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            var produto = await db.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (produto != null)
            {
                produto.Ativo = false;
                await db.SaveChangesAsync(ct);
            }
        }

        public async Task<decimal> ValorTotalEstoqueAsync(CancellationToken ct = default)
        {
            await using var db = _fabrica.Criar();
            // Realiza o cálculo diretamente no banco via SQL, evitando carregar tudo para a memória (AsEnumerable)
            return await db.Produtos
                .Where(p => p.Ativo)
                .SumAsync(p => p.PrecoCusto * p.Quantidade, ct);
        }
    }
}