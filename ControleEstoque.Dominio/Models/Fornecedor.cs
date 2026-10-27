using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Dominio.Models
{
    public class Fornecedor : IEmpresaEntity
    {
        [Key]
        public int Id { get; set; }

        // Empresa dona deste registro (preenchido automaticamente ao salvar)
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "O nome do fornecedor é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome do fornecedor não pode ultrapassar 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(18, ErrorMessage = "O CPF/CNPJ deve ter no máximo 18 caracteres.")]
        public string? CpfCnpj { get; set; }

        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string? Telefone { get; set; }

        [StringLength(20, ErrorMessage = "O WhatsApp deve ter no máximo 20 caracteres.")]
        public string? WhatsApp { get; set; }

        [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
        [StringLength(100, ErrorMessage = "O e-mail não pode ultrapassar 100 caracteres.")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "O endereço não pode ultrapassar 255 caracteres.")]
        public string? Endereco { get; set; }

        [StringLength(500, ErrorMessage = "As observações não podem ultrapassar 500 caracteres.")]
        public string? Observacoes { get; set; }

        // Propriedade de navegação
        public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
