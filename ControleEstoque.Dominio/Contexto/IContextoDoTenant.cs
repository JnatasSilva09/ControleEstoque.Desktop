namespace ControleEstoque.Dominio.Contexto
{
    // Diz quem está usando o sistema neste momento (empresa e usuário).
    // Os serviços e o banco só conhecem esta interface, nunca uma variável estática.
    // Na versão web, a implementação vai ler esses dados do token de cada requisição.
    public interface IContextoDoTenant
    {
        int EmpresaId { get; }
        int UsuarioId { get; }
    }
}
