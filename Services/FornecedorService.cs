using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    public class FornecedorService
    {
        public List<Fornecedor> GetAll(string? filtro = null)
        {
            using var db = new AppDbContext();
            var query = db.Fornecedores.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = filtro.Trim();
                query = query.Where(f => EF.Functions.Like(f.Nome, $"%{filtro}%"));
            }

            return query.OrderBy(f => f.Nome).ToList();
        }

        public Fornecedor? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Fornecedores.AsNoTracking().FirstOrDefault(f => f.Id == id);
        }

        public void Add(Fornecedor fornecedor)
        {
            using var db = new AppDbContext();

            fornecedor.Nome = fornecedor.Nome.Trim();

            // Valida se já existe um fornecedor cadastrado com o mesmo nome
            bool existe = db.Fornecedores.Any(f => f.Nome.ToLower() == fornecedor.Nome.ToLower());
            if (existe)
            {
                throw new InvalidOperationException($"Já existe um fornecedor cadastrado com o nome '{fornecedor.Nome}'.");
            }

            db.Fornecedores.Add(fornecedor);
            db.SaveChanges();
        }

        public void Update(Fornecedor fornecedor)
        {
            using var db = new AppDbContext();

            fornecedor.Nome = fornecedor.Nome.Trim();

            // Valida se já existe outro fornecedor com o mesmo nome
            bool existe = db.Fornecedores.Any(f => f.Nome.ToLower() == fornecedor.Nome.ToLower() && f.Id != fornecedor.Id);
            if (existe)
            {
                throw new InvalidOperationException($"Já existe outro fornecedor cadastrado com o nome '{fornecedor.Nome}'.");
            }

            db.Fornecedores.Update(fornecedor);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var fornecedor = db.Fornecedores.Find(id);

            if (fornecedor != null)
            {
                db.Fornecedores.Remove(fornecedor);
                db.SaveChanges();
            }
        }
    }
}