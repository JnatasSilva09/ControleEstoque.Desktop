using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public List<Produto> GetAll(string? filtro = null)
        {
            using var db = _fabrica.Criar();
            var query = db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .AsNoTracking()
                .Where(p => p.Ativo) // produtos inativados não aparecem na lista
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
            using var db = _fabrica.Criar();
            return db.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.Fornecedor)
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(Produto produto)
        {
            using var db = _fabrica.Criar();

            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
                produto.CodigoBarras = produto.CodigoBarras.Trim();

            // Confere as regras do modelo (campos obrigatórios, tamanhos, valores negativos...)
            ValidacaoHelper.Garantir(produto);

            // Validação de unicidade do código do produto
            var existente = db.Produtos.FirstOrDefault(p => p.Codigo.ToLower() == produto.Codigo.ToLower());
            if (existente != null)
            {
                var aviso = existente.Ativo ? "" : " (o produto está inativo)";
                throw new InvalidOperationException($"Já existe um produto cadastrado com o código '{produto.Codigo}'{aviso}.");
            }

            produto.Ativo = true;

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
            using var db = _fabrica.Criar();

            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
            if (!string.IsNullOrWhiteSpace(produto.CodigoBarras))
                produto.CodigoBarras = produto.CodigoBarras.Trim();

            ValidacaoHelper.Garantir(produto);

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

            // Carrega o registro que já existe e copia só os campos editáveis
            // (assim a data de cadastro, a empresa e a situação do produto não são sobrescritas)
            var atual = db.Produtos.FirstOrDefault(p => p.Id == produto.Id)
                ?? throw new InvalidOperationException("Produto não encontrado.");

            atual.Codigo = produto.Codigo;
            atual.CodigoBarras = produto.CodigoBarras;
            atual.Nome = produto.Nome;
            atual.Descricao = produto.Descricao;
            atual.UnidadeMedida = produto.UnidadeMedida;
            atual.PrecoCusto = produto.PrecoCusto;
            atual.PrecoVenda = produto.PrecoVenda;
            // Quantidade NÃO é copiada aqui de propósito: o estoque só muda por entrada, saída ou ajuste.         
            atual.EstoqueMinimo = produto.EstoqueMinimo;
            atual.CategoriaId = produto.CategoriaId;
            atual.FornecedorId = produto.FornecedorId;

            db.SaveChanges();
        }

        // Em vez de apagar, o produto é inativado: assim o histórico de movimentações continua intacto
        public void Delete(int id)
        {
            using var db = _fabrica.Criar();
            var produto = db.Produtos.FirstOrDefault(p => p.Id == id);
            if (produto != null)
            {
                produto.Ativo = false;
                db.SaveChanges();
            }
        }

        public List<Produto> GetEstoqueBaixo()
        {
            using var db = _fabrica.Criar();
            return db.Produtos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .Where(p => p.Ativo && p.Quantidade > 0 && p.Quantidade <= p.EstoqueMinimo)
                .OrderBy(p => p.Quantidade)
                .ToList();
        }

        public List<Produto> GetSemEstoque()
        {
            using var db = _fabrica.Criar();
            return db.Produtos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .Where(p => p.Ativo && p.Quantidade <= 0)
                .OrderBy(p => p.Nome)
                .ToList();
        }

        public int TotalProdutos()
        {
            using var db = _fabrica.Criar();
            return db.Produtos.Count(p => p.Ativo);
        }

        public decimal ValorTotalEstoque()
        {
            using var db = _fabrica.Criar();
            return db.Produtos
                     .Where(p => p.Ativo)
                     .AsEnumerable()
                     .Sum(p => p.PrecoCusto * p.Quantidade);
        }
    }
}
