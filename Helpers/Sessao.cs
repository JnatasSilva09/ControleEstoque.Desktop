using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Helpers
{
    // Guarda quem está logado no momento. O AppDbContext usa o EmpresaId daqui
    // para mostrar (e salvar) somente os dados da empresa do usuário.
    public static class Sessao
    {
        public static int EmpresaId { get; private set; }
        public static int UsuarioId { get; private set; }

        public static void Iniciar(Usuario usuario)
        {
            EmpresaId = usuario.EmpresaId;
            UsuarioId = usuario.Id;
        }

        public static void Encerrar()
        {
            EmpresaId = 0;
            UsuarioId = 0;
        }
    }
}
