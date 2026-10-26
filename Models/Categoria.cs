using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Desktop.Models
{
    public class Categoria : IEmpresaEntity
    {
        [Key]
        public int Id { get; set; }

        // Empresa dona deste registro (preenchido automaticamente ao salvar)
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da categoria não pode ultrapassar 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "A descrição não pode ultrapassar 255 caracteres.")]
        public string? Descricao { get; set; }

        // Propriedade de navegação
        public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}