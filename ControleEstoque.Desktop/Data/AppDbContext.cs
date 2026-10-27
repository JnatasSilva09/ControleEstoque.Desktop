using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<Empresa> Empresas { get; set; } // Adicionado para resolver o erro do AuthService

        private readonly string _caminhoBanco;

        public AppDbContext(IContextoDoTenant tenant, string? caminhoBanco = null)
        {
            // Empresa de quem está logado. É lida na hora em que o contexto é criado
            // e usada nos filtros abaixo, assim cada empresa só enxerga os próprios dados.
            EmpresaIdAtual = tenant.EmpresaId;
            _caminhoBanco = caminhoBanco ?? CaminhoBanco;
        }

        public int EmpresaIdAtual { get; }

        public static string PastaDados => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ControleDeEstoque");

        // Caminho padrão do banco de verdade
        public static string CaminhoBanco => Path.Combine(PastaDados, "estoque.db");

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var pasta = Path.GetDirectoryName(_caminhoBanco);
            if (!string.IsNullOrEmpty(pasta) && !Directory.Exists(pasta))
            {
                Directory.CreateDirectory(pasta);
            }

            optionsBuilder.UseSqlite($"Data Source={_caminhoBanco};Foreign Keys=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Filtros: cada consulta só traz os registros da empresa logada
            modelBuilder.Entity<Produto>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Categoria>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Fornecedor>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Movimentacao>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Configuracao>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);

            // O código do produto não pode se repetir dentro da mesma empresa
            modelBuilder.Entity<Produto>()
                .HasIndex(p => new { p.EmpresaId, p.Codigo })
                .IsUnique();

            // O login é usado para entrar no sistema, então não pode se repetir
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();
        }

        // Antes de salvar, garante que todo registro novo fica na empresa logada
        // e que ninguém consegue alterar um registro de outra empresa.
        private void AplicarEmpresaAtual()
        {
            foreach (var entrada in ChangeTracker.Entries<IEmpresaEntity>())
            {
                if (entrada.State == EntityState.Added)
                {
                    if (EmpresaIdAtual == 0)
                        throw new InvalidOperationException("Nenhuma empresa ativa na sessão. Faça login novamente.");

                    entrada.Entity.EmpresaId = EmpresaIdAtual;
                }
                else if (entrada.State == EntityState.Modified || entrada.State == EntityState.Deleted)
                {
                    if (entrada.Entity.EmpresaId != EmpresaIdAtual)
                        throw new InvalidOperationException("Operação não permitida para este registro.");
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AplicarEmpresaAtual();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AplicarEmpresaAtual();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
