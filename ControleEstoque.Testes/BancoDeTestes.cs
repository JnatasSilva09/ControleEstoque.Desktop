using Npgsql;

namespace ControleEstoque.Testes;

// Cada teste usa um banco PostgreSQL novo, com nome único, que é apagado no final.
// O banco de desenvolvimento (controle_estoque) nunca é tocado.
public static class BancoDeTestes
{
    // Ordem: variável ESTOQUE_TESTES_CONEXAO (usada no CI) -> arquivo .env do repositório (uso local)
    public static string ConexaoBase()
    {
        var env = Environment.GetEnvironmentVariable("ESTOQUE_TESTES_CONEXAO");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        var senha = LerSenhaDoArquivoEnv()
            ?? throw new InvalidOperationException(
                "Não achei a senha do PostgreSQL. Crie o arquivo .env na raiz (veja .env.example) " +
                "ou defina a variável ESTOQUE_TESTES_CONEXAO.");

        return $"Host=localhost;Port=5432;Username=estoque;Password={senha}";
    }

    public static string NovaConexao()
    {
        var builder = new NpgsqlConnectionStringBuilder(ConexaoBase())
        {
            Database = $"estoque_teste_{Guid.NewGuid():N}"
        };
        return builder.ConnectionString;
    }

    private static string? LerSenhaDoArquivoEnv()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta != null)
        {
            var arquivo = Path.Combine(pasta.FullName, ".env");
            if (File.Exists(arquivo))
            {
                foreach (var linha in File.ReadAllLines(arquivo))
                {
                    if (linha.StartsWith("POSTGRES_PASSWORD=", StringComparison.Ordinal))
                        return linha["POSTGRES_PASSWORD=".Length..].Trim();
                }
            }
            pasta = pasta.Parent;
        }
        return null;
    }
}
