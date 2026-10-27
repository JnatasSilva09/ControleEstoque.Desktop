namespace ControleEstoque.Dominio.Models
{
    // Marca os registros que pertencem a uma empresa (cada empresa só enxerga os seus dados).
    public interface IEmpresaEntity
    {
        int EmpresaId { get; set; }
    }
}
