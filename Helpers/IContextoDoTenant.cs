using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Helpers
{
    // Diz quem está usando o sistema neste momento (empresa e usuário).
    // Os serviços e o banco só conhecem esta interface, nunca uma variável estática.
    // Na versão web, a implementação vai ler esses dados do token de cada requisição.
    public interface IContextoDoTenant
    {
        int EmpresaId { get; }
        int UsuarioId { get; }
    }

    // Versão do desktop: um usuário por vez, definido no login e limpo no logout.
    public class ContextoDoTenantDesktop : IContextoDoTenant
    {
        public int EmpresaId { get; private set; }
        public int UsuarioId { get; private set; }

        public void Iniciar(Usuario usuario)
        {
            EmpresaId = usuario.EmpresaId;
            UsuarioId = usuario.Id;
        }

        public void Encerrar()
        {
            EmpresaId = 0;
            UsuarioId = 0;
        }
    }
}
