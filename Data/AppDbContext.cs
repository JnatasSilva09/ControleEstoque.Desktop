using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Fornecedor> Fornecedores { get; set; } = null!;
        public DbSet<Movimentacao> Movimentacoes { get; set; } = null!;
        public DbSet<Configuracao> Configuracoes { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Define o caminho seguro na pasta AppData/Local do utilizador
                string pastaAppData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ControleEstoque"
                );

                // Garante que a pasta existe no computador do cliente
                if (!Directory.Exists(pastaAppData))
                {
                    Directory.CreateDirectory(pastaAppData);
                }

                string caminhoBanco = Path.Combine(pastaAppData, "estoque.db");

                // Configura o SQLite para utilizar o caminho absoluto e ativar as Foreign Keys
                optionsBuilder.UseSqlite($"Data Source={caminhoBanco};Foreign Keys=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Um produto pode ter uma categoria (opcional).
            // Se a categoria for excluída, o produto continua existindo, só perde a categoria.
            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Um produto pode ter um fornecedor (opcional), mesma regra acima.
            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Fornecedor)
                .WithMany(f => f.Produtos)
                .HasForeignKey(p => p.FornecedorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Uma movimentação sempre pertence a um produto.
            // Se o produto for excluído, o histórico de movimentações dele também é removido.
            modelBuilder.Entity<Movimentacao>()
                .HasOne(m => m.Produto)
                .WithMany()
                .HasForeignKey(m => m.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Não pode existir dois usuários com o mesmo login.
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();
        }
    }
}