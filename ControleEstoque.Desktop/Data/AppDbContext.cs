using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Models;
using System;
using System.IO;

namespace ControleEstoque.Desktop.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Configuracao> Configuracoes { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Empresa> Empresas { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Movimentacao> Movimentacoes { get; set; } = null!;
        public DbSet<Fornecedor> Fornecedores { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string pastaAppData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ControleEstoque"
                );

                if (!Directory.Exists(pastaAppData))
                {
                    Directory.CreateDirectory(pastaAppData);
                }

                string caminhoBanco = Path.Combine(pastaAppData, "estoque.db");
                optionsBuilder.UseSqlite($"Data Source={caminhoBanco};Foreign Keys=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}