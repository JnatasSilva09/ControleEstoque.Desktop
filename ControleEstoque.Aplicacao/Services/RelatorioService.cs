using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ControleEstoque.Aplicacao.Services
{
    public class RelatorioService
    {
        /// <summary>
        /// Gera uma string formatada em CSV a partir de uma lista de dados.
        /// </summary>
        public static string ExportarMovimentacoesParaCsv<T>(IEnumerable<T> dados)
        {
            var sb = new StringBuilder();
            var propriedades = typeof(T).GetProperties();

            // Cabeçalho do CSV
            var cabecalho = string.Join(";", propriedades.Select(p => p.Name));
            sb.AppendLine(cabecalho);

            // Linhas de dados
            foreach (var item in dados)
            {
                var valores = propriedades.Select(p => 
                {
                    var valor = p.GetValue(item, null);
                    if (valor == null) return string.Empty;

                    string valorStr = valor.ToString()?.Replace("\"", "\"\"") ?? string.Empty;

                    // Evita que o Excel execute como fórmula um texto que comece com = + - @
                    // (proteção contra "injeção de fórmulas" em textos digitados pelos usuários)
                    if (valorStr.Length > 0 && "=+-@\t\r".Contains(valorStr[0]) && valor is string)
                        valorStr = "'" + valorStr;
                    return $"\"{valorStr}\"";
                });

                sb.AppendLine(string.Join(";", valores));
            }

            return sb.ToString();
        }
    }
}