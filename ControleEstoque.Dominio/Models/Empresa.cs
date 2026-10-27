using System;

namespace ControleEstoque.Dominio.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public StatusAssinatura StatusAssinatura { get; set; }
        public DateTime DataVencimento { get; set; }
    }

    public enum StatusAssinatura
    {
        Degustacao = 0,
        Ativa = 1,
        AguardandoPagamento = 2,
        Bloqueada = 3
    }
}