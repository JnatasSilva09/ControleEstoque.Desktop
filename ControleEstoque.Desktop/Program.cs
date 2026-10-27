using System;
using System.IO;
using System.Windows.Forms;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Forms; // Importa o namespace onde estão os seus Forms
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Qualquer erro que ninguém tratou vai para o log, em vez de fechar o sistema sem explicação
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                LogHelper.Erro(e.Exception, "Erro não tratado");
                MessageBox.Show("Ocorreu um erro inesperado. Os detalhes foram gravados no arquivo de log.",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    LogHelper.Erro(ex, "Erro fatal");
            };

            try
            {
                // Garante que o banco de dados e todas as tabelas (incluindo Empresas) são criados automaticamente
                bool bancoJaExistia = File.Exists(AppDbContext.CaminhoBanco);

                // Se já existe um banco, guarda uma cópia antes de mexer em qualquer coisa
                if (bancoJaExistia)
                {
                    BackupService.FazerBackupAntesDeAtualizar();
                }

                using (var context = new AppDbContext())
                {
                    context.Database.EnsureCreated();

                    // Bancos antigos recebem as colunas e tabelas novas sem perder dados
                    if (bancoJaExistia)
                    {
                        AtualizadorBanco.Atualizar(context);
                    }
                }

                BackupService.FazerBackupAutomatico();
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Inicialização do banco de dados");
                MessageBox.Show($"Não foi possível iniciar o banco de dados.\n\n{ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Repete login -> sistema enquanto o usuário escolher "Sair" (voltar para o login)
            while (true)
            {
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() != DialogResult.OK || loginForm.UsuarioAutenticado == null)
                {
                    break;
                }

                Sessao.Iniciar(loginForm.UsuarioAutenticado);

                var mainForm = new MainForm(loginForm.UsuarioAutenticado);
                Application.Run(mainForm);

                bool voltarParaLogin = mainForm.SolicitouLogout;
                Sessao.Encerrar();

                if (!voltarParaLogin)
                {
                    break;
                }
            }
        }
    }
}
