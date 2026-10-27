using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Helpers;

namespace ControleEstoque.Desktop.Data
{
    // O EnsureCreated() só cria o banco quando ele ainda não existe. Se você mudar um modelo
    // depois (como adicionar EmpresaId), o banco antigo ficaria diferente do código.
    // Esta classe pega o banco antigo e acrescenta o que estiver faltando, sem apagar nada.
    public static class AtualizadorBanco
    {
        public static void Atualizar(AppDbContext db)
        {
            var conexao = db.Database.GetDbConnection();
            conexao.Open();

            try
            {
                var comandos = db.Database.GenerateCreateScript()
                    .Replace("\r\n", "\n")
                    .Split(";\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => c.Length > 0)
                    .ToList();

                // 1. Cria as tabelas que ainda não existem
                foreach (var comando in comandos.Where(c => c.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase)))
                {
                    Executar(conexao,
                        "CREATE TABLE IF NOT EXISTS" + comando.Substring("CREATE TABLE".Length));
                }

                // 2. Se já existiam dados antes do controle por empresa, eles passam a
                //    pertencer a uma empresa padrão (com assinatura ativa, para ninguém ficar bloqueado)
                Executar(conexao, @"INSERT INTO ""Empresas"" (""NomeEmpresa"", ""StatusAssinatura"", ""DataVencimento"")
                    SELECT COALESCE((SELECT ""NomeEmpresa"" FROM ""Configuracoes"" LIMIT 1), 'Minha Empresa'), 1, '2099-12-31 00:00:00'
                    WHERE NOT EXISTS (SELECT 1 FROM ""Empresas"")
                      AND (EXISTS (SELECT 1 FROM ""Usuarios"") OR EXISTS (SELECT 1 FROM ""Produtos""));");

                var empresaPadrao = ObterEmpresaPadrao(conexao);

                // 3. Acrescenta as colunas novas nas tabelas antigas
                GarantirColuna(conexao, "Usuarios", "EmpresaId", $"INTEGER NOT NULL DEFAULT {empresaPadrao}");
                GarantirColuna(conexao, "Usuarios", "PrecisaTrocarSenha", "INTEGER NOT NULL DEFAULT 0");
                GarantirColuna(conexao, "Produtos", "EmpresaId", $"INTEGER NOT NULL DEFAULT {empresaPadrao}");
                GarantirColuna(conexao, "Produtos", "Ativo", "INTEGER NOT NULL DEFAULT 1");
                GarantirColuna(conexao, "Categorias", "EmpresaId", $"INTEGER NOT NULL DEFAULT {empresaPadrao}");
                GarantirColuna(conexao, "Fornecedores", "EmpresaId", $"INTEGER NOT NULL DEFAULT {empresaPadrao}");
                GarantirColuna(conexao, "Movimentacoes", "EmpresaId", $"INTEGER NOT NULL DEFAULT {empresaPadrao}");
                GarantirColuna(conexao, "Configuracoes", "EmpresaId", $"INTEGER NOT NULL DEFAULT {empresaPadrao}");

                // 4. Índices (por último, porque dependem das colunas acima).
                //    Se já existirem dados repetidos, o índice único não é criado e o erro vai para o log.
                foreach (var comando in comandos.Where(c => c.StartsWith("CREATE INDEX", StringComparison.OrdinalIgnoreCase)
                                                         || c.StartsWith("CREATE UNIQUE INDEX", StringComparison.OrdinalIgnoreCase)))
                {
                    var comIfNotExists = comando.Replace("INDEX ", "INDEX IF NOT EXISTS ");
                    try
                    {
                        Executar(conexao, comIfNotExists);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Erro(ex, $"Não foi possível criar o índice: {comando}");
                    }
                }
            }
            finally
            {
                conexao.Close();
            }
        }

        private static long ObterEmpresaPadrao(DbConnection conexao)
        {
            using var cmd = conexao.CreateCommand();
            cmd.CommandText = @"SELECT MIN(""Id"") FROM ""Empresas""";
            var resultado = cmd.ExecuteScalar();
            return resultado == null || resultado is DBNull ? 1 : Convert.ToInt64(resultado);
        }

        private static void GarantirColuna(DbConnection conexao, string tabela, string coluna, string definicao)
        {
            if (ColunaExiste(conexao, tabela, coluna)) return;
            Executar(conexao, $@"ALTER TABLE ""{tabela}"" ADD COLUMN ""{coluna}"" {definicao};");
        }

        private static bool ColunaExiste(DbConnection conexao, string tabela, string coluna)
        {
            using var cmd = conexao.CreateCommand();
            cmd.CommandText = $@"PRAGMA table_info(""{tabela}"")";
            using var leitor = cmd.ExecuteReader();
            while (leitor.Read())
            {
                if (string.Equals(leitor.GetString(1), coluna, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void Executar(DbConnection conexao, string sql)
        {
            using var cmd = conexao.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
