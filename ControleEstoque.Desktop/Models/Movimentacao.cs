using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleEstoque.Desktop.Models
{
    public enum TipoMovimentacao
    {
        Entrada,
        Saida
    }

    public class Movimentacao : IEmpresaEntity
    {
        [Key]
        public int Id { get; set; }

        // Empresa dona deste registro (preenchido automaticamente ao salvar)
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório.")]
        public int ProdutoId { get; set; }

        public virtual Produto? Produto { get; set; }

        [Required(ErrorMessage = "O tipo de movimentação é obrigatório.")]
        public TipoMovimentacao Tipo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser de pelo menos 1 unidade.")]
        public int Quantidade { get; set; }

        // Preenchido apenas em entradas
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "O valor de custo deve ser maior que zero.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorCusto { get; set; }

        // Preenchido apenas em entradas (nome do fornecedor digitado na hora)
        [StringLength(150, ErrorMessage = "O nome do fornecedor não pode ultrapassar 150 caracteres.")]
        public string? FornecedorNome { get; set; }

        // Preenchido apenas em saídas (ex: "Venda", "Perda", "Uso interno")
        [StringLength(255, ErrorMessage = "O motivo não pode ultrapassar 255 caracteres.")]
        public string? Motivo { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;
    }
}