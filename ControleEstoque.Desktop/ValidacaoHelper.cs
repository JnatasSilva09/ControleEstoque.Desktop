using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Desktop.Helpers;

public static class ValidacaoHelper
{
    public static void Garantir<T>(T entidade) where T : notnull
    {
        var validacoes = new List<ValidationResult>();
        var contexto = new ValidationContext(entidade, serviceProvider: null, items: null);

        bool isValid = Validator.TryValidateObject(entidade, contexto, validacoes, validateAllProperties: true);

        if (!isValid)
        {
            var mensagens = string.Join(Environment.NewLine, validacoes.Select(v => v.ErrorMessage));
            // Lança uma exceção limpa que pode ser tratada na camada de apresentação
            throw new ValidationException(mensagens);
        }
    }
}