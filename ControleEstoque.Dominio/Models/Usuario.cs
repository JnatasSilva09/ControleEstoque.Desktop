using System;
using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Dominio.Models
{
    public enum PerfilUsuario
    {
        Administrador,
        Operador
    }

    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode ultrapassar 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O login de acesso é obrigatório.")]
        [StringLength(50, ErrorMessage = "O login não pode ultrapassar 50 caracteres.")]
        public string Login { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string SenhaHash { get; set; } = string.Empty;

        // Não é mais usado: o BCrypt já guarda o salt dentro do próprio SenhaHash.
        // A propriedade continua aqui só para não quebrar bancos que já têm essa coluna.
        [StringLength(255)]
        public string SenhaSalt { get; set; } = string.Empty;

        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Operador;

        public bool Ativo { get; set; } = true;

        // Quando for true, o usuário precisa criar uma senha nova no próximo login
        // (usado quando o administrador cria o usuário ou redefine a senha dele)
        public bool PrecisaTrocarSenha { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    
    // Chave estrangeira para a Empresa (Inquilino)
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
}
