using ControleEstoque.Dominio.Contexto;

namespace ControleEstoque.Infra.Data
{
    // Os serviços pedem um contexto novo a cada operação (curto e descartável),
    // então recebem esta fábrica em vez de criar o AppDbContext por conta própria.
    public interface IAppDbContextFactory
    {
        AppDbContext Criar();
    }

    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly IContextoDoTenant _tenant;
        private readonly string _conexao;

        // conexao = string de conexão do PostgreSQL (nos testes, aponta para um banco descartável)
        public AppDbContextFactory(IContextoDoTenant tenant, string conexao)
        {
            _tenant = tenant;
            _conexao = conexao;
        }

        public AppDbContext Criar() => new AppDbContext(_tenant, _conexao);
    }
}
