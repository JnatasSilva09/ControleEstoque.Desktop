using ControleEstoque.Dominio.Contexto;
using ControleEstoque.Dominio.Models;

namespace ControleEstoque.Desktop.Helpers
{
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
