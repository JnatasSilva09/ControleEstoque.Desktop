using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Desktop.Models
{
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O código interno é obrigatório.")]
        [StringLength(50, ErrorMessage = "O código não pode ultrapassar 50 caracteres.")]
        public string Codigo { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "O código de barras não pode ultrapassar 100 caracteres.")]
        public string? CodigoBarras { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome do produto não pode ultrapassar 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição não pode ultrapassar 500 caracteres.")]
        public string? Descricao { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "A unidade de medida deve ter no máximo 10 caracteres.")]
        public string UnidadeMedida { get; set; } = "UN";

        [Range(0.00, (double)decimal.MaxValue, ErrorMessage = "O preço de custo não pode ser negativo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoCusto { get; set; }

        [Range(0.00, (double)decimal.MaxValue, ErrorMessage = "O preço de venda não pode ser negativo.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoVenda { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque não pode ser negativa.")]
        public int Quantidade { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O estoque mínimo não pode ser negativo.")]
        public int EstoqueMinimo { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Relacionamentos
        public int? CategoriaId { get; set; }
        public virtual Categoria? Categoria { get; set; }

        public int? FornecedorId { get; set; }
        public virtual Fornecedor? Fornecedor { get; set; }

        // Propriedade calculada (não vira coluna no banco)
        [NotMapped]
        public string SituacaoEstoque
        {
            get
            {
                if (Quantidade <= 0) return "SEM ESTOQUE";
                if (Quantidade <= EstoqueMinimo) return "ESTOQUE BAIXO";
                return "OK";
            }
        }
    }
}