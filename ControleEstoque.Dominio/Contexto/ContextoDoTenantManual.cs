using ControleEstoque.Dominio.Models;

namespace ControleEstoque.Dominio.Contexto
{
    // Um usuário por vez, definido no login e limpo no logout.
    // Serve ao desktop e aos testes. A API (Fase 3) terá outra versão que lê o token do usuário.
    public class ContextoDoTenantManual : IContextoDoTenant
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
