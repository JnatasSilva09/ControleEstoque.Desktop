using System;
using System.IO;

namespace ControleEstoque.Desktop.Helpers
{
    // Grava os erros em um arquivo de texto para facilitar descobrir o que deu errado.
    // Nunca grave senhas ou dados sensíveis aqui.
    public static class LogHelper
    {
        private static string PastaLogs => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ControleDeEstoque", "logs");

        public static void Erro(Exception ex, string? contexto = null)
        {
            try
            {
                Directory.CreateDirectory(PastaLogs);
                var arquivo = Path.Combine(PastaLogs, $"erro-{DateTime.Now:yyyyMMdd}.log");
                var texto = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {contexto}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
                File.AppendAllText(arquivo, texto);
            }
            catch
            {
                // Se não der para gravar o log, não vamos travar o sistema por causa disso
            }
        }
    }
}
