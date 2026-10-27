using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Dominio.Models
{
    public class Configuracao : IEmpresaEntity
    {
        [Key]
        public int Id { get; set; }

        // Empresa dona deste registro (preenchido automaticamente ao salvar)
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome da empresa não pode ultrapassar 150 caracteres.")]
        public string NomeEmpresa { get; set; } = string.Empty;

        [StringLength(18, ErrorMessage = "O CNPJ deve ter no máximo 18 caracteres.")] // Ex: 00.000.000/0001-00
        public string? Cnpj { get; set; }

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }

        [StringLength(20, ErrorMessage = "O WhatsApp deve ter no máximo 20 caracteres.")]
        public string? WhatsApp { get; set; }

        [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
        [StringLength(100, ErrorMessage = "O e-mail não pode ultrapassar 100 caracteres.")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "O endereço não pode ultrapassar 255 caracteres.")]
        public string? Endereco { get; set; }

        // Logo salvo como bytes da imagem dentro do próprio banco SQLite
        public byte[]? Logo { get; set; }
    }
}