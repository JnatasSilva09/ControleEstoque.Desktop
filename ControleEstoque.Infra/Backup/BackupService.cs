using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Infra.Data;
using ControleEstoque.Infra.Helpers;

namespace ControleEstoque.Infra.Backups
{
    public class BackupService
    {
        private const int QuantidadeBackupsAutomaticos = 10;

        private readonly IAppDbContextFactory _fabrica;

        public BackupService(IAppDbContextFactory fabrica)
        {
            _fabrica = fabrica;
        }

        public static string PastaBackups => Path.Combine(AppDbContext.PastaDados, "backups");

        // Salva uma cópia do banco no caminho informado.
        public void SalvarCopia(string destino)
        {
            if (File.Exists(destino))
                File.Delete(destino);

            try
            {
                // VACUUM INTO gera uma cópia consistente, mesmo com o banco em uso
                using var db = _fabrica.Criar();
                var conexao = db.Database.GetDbConnection();
                conexao.Open();
                try
                {
                    using var cmd = conexao.CreateCommand();
                    cmd.CommandText = $"VACUUM INTO '{destino.Replace("'", "''")}'";
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    conexao.Close();
                }
            }
            catch (Exception ex)
            {
                // Plano B: copia o arquivo direto
                LogHelper.Erro(ex, "VACUUM INTO falhou, usando cópia simples do arquivo");
                File.Copy(AppDbContext.CaminhoBanco, destino, true);
            }
        }

        // Backup feito antes de mexer na estrutura do banco
        public void FazerBackupAntesDeAtualizar()
        {
            Directory.CreateDirectory(PastaBackups);
            SalvarCopia(Path.Combine(PastaBackups, $"antes-de-atualizar-{DateTime.Now:yyyyMMdd-HHmmss}.db"));
        }

        // Um backup por dia, guardando só os mais recentes
        public void FazerBackupAutomatico()
        {
            try
            {
                if (!File.Exists(AppDbContext.CaminhoBanco)) return;

                Directory.CreateDirectory(PastaBackups);
                var arquivoDeHoje = Path.Combine(PastaBackups, $"backup-{DateTime.Now:yyyyMMdd}.db");
                if (File.Exists(arquivoDeHoje)) return;

                SalvarCopia(arquivoDeHoje);

                var antigos = new DirectoryInfo(PastaBackups)
                    .GetFiles("backup-*.db")
                    .OrderByDescending(f => f.Name)
                    .Skip(QuantidadeBackupsAutomaticos);

                foreach (var arquivo in antigos)
                    arquivo.Delete();
            }
            catch (Exception ex)
            {
                // Falha no backup não deve impedir o uso do sistema
                LogHelper.Erro(ex, "Backup automático");
            }
        }
    }
}
