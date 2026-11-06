using System;
using Microsoft.Extensions.Configuration;

namespace ControleEstoque.Desktop.Helpers
{
    // Descobre a string de conexão do PostgreSQL sem deixar senha dentro do código.
    // Ordem (quem vem depois vence): appsettings.json -> user-secrets -> variável de ambiente.
    public static class ConfiguracaoBanco
    {
        public static string ObterConexao()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets(typeof(ConfiguracaoBanco).Assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conexao = config.GetConnectionString("Padrao");
            if (string.IsNullOrWhiteSpace(conexao))
                throw new InvalidOperationException(
                    "String de conexão não configurada. Veja as instruções do user-secrets no README.");

            return conexao;
        }
    }
}
