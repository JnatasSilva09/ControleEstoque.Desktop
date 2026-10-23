using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    public class ProdutoService
    {
        public List<Produto> GetAll(string? filtro = null)
        {
            using var db = new AppDbContext();
            var query = db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var f = filtro.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Nome, $"%{f}%") ||
                    EF.Functions.Like(p.Codigo, $"%{f}%") ||
                    (p.CodigoBarras != null && EF.Functions.Like(p.CodigoBarras, $"%{f}%")));
            }

            return query.OrderBy(p => p.Nome).ToList();
        }

        public Produto? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(Produto produto)
        {
            using var db = new AppDbContext();

            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
                produto.CodigoBarras = produto.CodigoBarras.Trim();

            // Validação de unicidade do código do produto
            bool codigoExiste = db.Produtos.Any(p => p.Codigo.ToLower() == produto.Codigo.ToLower());
            if (codigoExiste)
            {
                throw new InvalidOperationException($"Já existe um produto cadastrado com o código '{produto.Codigo}'.");
            }

            // Validação de unicidade do código de barras (se informado)
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
            {
                bool codigoBarrasExiste = db.Produtos.Any(p => p.CodigoBarras != null && p.CodigoBarras.ToLower() == produto.CodigoBarras.ToLower());
                if (codigoBarrasExiste)
                {
                    throw new InvalidOperationException($"Já existe um produto cadastrado com o código de barras '{produto.CodigoBarras}'.");
                }
            }

            db.Produtos.Add(produto);
            db.SaveChanges();
        }

        public void Update(Produto produto)
        {
            using var db = new AppDbContext();

            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
                produto.CodigoBarras = produto.CodigoBarras.Trim();

            // Validação de unicidade do código
            bool codigoExiste = db.Produtos.Any(p => p.Codigo.ToLower() == produto.Codigo.ToLower() && p.Id != produto.Id);
            if (codigoExiste)
            {
                throw new InvalidOperationException($"Já existe outro produto cadastrado com o código '{produto.Codigo}'.");
            }

            // Validação de unicidade do código de barras
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
            {
                bool codigoBarrasExiste = db.Produtos.Any(p => p.CodigoBarras != null && p.CodigoBarras.ToLower() == produto.CodigoBarras.ToLower() && p.Id != produto.Id);
                if (codigoBarrasExiste)
                {
                    throw new InvalidOperationException($"Já existe outro produto cadastrado com o código de barras '{produto.CodigoBarras}'.");
                }
            }

            db.Produtos.Update(produto);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var produto = db.Produtos.Find(id);
            if (produto != null)
            {
                db.Produtos.Remove(produto);
                db.SaveChanges();
            }
        }

        public List<Produto> GetEstoqueBaixo()
        {
            using var db = new AppDbContext();
            return db.Produtos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .Where(p => p.Quantidade > 0 && p.Quantidade <= p.EstoqueMinimo)
                .OrderBy(p => p.Quantidade)
                .ToList();
        }

        public List<Produto> GetSemEstoque()
        {
            using var db = new AppDbContext();
            return db.Produtos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .Where(p => p.Quantidade <= 0)
                .OrderBy(p => p.Nome)
                .ToList();
        }

        public int TotalProdutos()
        {
            using var db = new AppDbContext();
            return db.Produtos.Count();
        }

        public decimal ValorTotalEstoque()
{
    using var db = new AppDbContext();
    return db.Produtos
             .AsEnumerable()
             .Sum(p => p.PrecoCusto * p.Quantidade); //[cite: 5, 6]
}
    }
}
