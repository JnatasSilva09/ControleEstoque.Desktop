using System.Text.RegularExpressions;
using System.Globalization;

namespace ControleEstoque.Desktop.Helpers;

public static class FormatHelper
{
    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    public static string Moeda(decimal valor)
    {
        return valor.ToString("C2", PtBr);
    }

    public static string DataCurta(DateTime data)
    {
        return data.ToString("dd/MM/yyyy", PtBr);
    }

    public static string DataHora(DateTime data)
    {
        return data.ToString("dd/MM/yyyy HH:mm", PtBr);
    }

    public static bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string padrao = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, padrao);
    }
}