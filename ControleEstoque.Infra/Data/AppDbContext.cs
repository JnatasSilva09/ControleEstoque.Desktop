using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using ControleEstoque.Dominio.Contexto;
using ControleEstoque.Dominio.Models;

namespace ControleEstoque.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<Empresa> Empresas { get; set; }

        private readonly string _conexao;

        static AppDbContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public AppDbContext(IContextoDoTenant tenant, string conexao)
        {
            EmpresaIdAtual = tenant.EmpresaId;
            _conexao = conexao;
        }

        public int EmpresaIdAtual { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_conexao);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Habilita a extensão citext do PostgreSQL para buscas e restrições case-insensitive performáticas
            modelBuilder.HasPostgresExtension("citext");

            modelBuilder.Entity<Usuario>().Property(u => u.Login).HasColumnType("citext");
            modelBuilder.Entity<Produto>().Property(p => p.Codigo).HasColumnType("citext");
            modelBuilder.Entity<Produto>().Property(p => p.CodigoBarras).HasColumnType("citext");

            // Mantém os filtros globais por empresa existentes
            modelBuilder.Entity<Produto>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Categoria>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Fornecedor>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Movimentacao>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);
            modelBuilder.Entity<Configuracao>().HasQueryFilter(e => e.EmpresaId == EmpresaIdAtual);

            modelBuilder.Entity<Produto>()
                .HasIndex(p => new { p.EmpresaId, p.Codigo })
                .IsUnique();

            modelBuilder.Entity<Produto>()
                .HasIndex(p => new { p.EmpresaId, p.CodigoBarras })
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();
        }

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