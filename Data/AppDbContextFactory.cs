using ControleEstoque.Desktop.Helpers;

namespace ControleEstoque.Desktop.Data
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
        private readonly string? _caminhoBanco;

        // caminhoBanco só é informado nos testes, para não encostar no banco de verdade
        public AppDbContextFactory(IContextoDoTenant tenant, string? caminhoBanco = null)
        {
            _tenant = tenant;
            _caminhoBanco = caminhoBanco;
        }

        public AppDbContext Criar() => new AppDbContext(_tenant, _caminhoBanco);
    }
}
