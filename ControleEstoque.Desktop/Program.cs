using System;
using System.IO;
using System.Windows.Forms;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Desktop.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Infra.Data;
using ControleEstoque.Infra.Helpers;

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

            // Monta uma vez só o que as telas e os serviços vão usar
            var tenant = new ContextoDoTenantDesktop();
            var fabrica = new AppDbContextFactory(tenant);
            var servicos = new Servicos(fabrica, tenant);

            try
            {
                // Garante que o banco de dados e todas as tabelas (incluindo Empresas) são criados automaticamente
                bool bancoJaExistia = File.Exists(AppDbContext.CaminhoBanco);

                // Se já existe um banco, guarda uma cópia antes de mexer em qualquer coisa
                if (bancoJaExistia)
                {
                    servicos.Backup.FazerBackupAntesDeAtualizar();
                }

                using (var context = fabrica.Criar())
                {
                    context.Database.EnsureCreated();

                    // Bancos antigos recebem as colunas e tabelas novas sem perder dados
                    if (bancoJaExistia)
                    {
                        AtualizadorBanco.Atualizar(context);
                    }
                }

                servicos.Backup.FazerBackupAutomatico();
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
                var loginForm = new LoginForm(servicos);
                if (loginForm.ShowDialog() != DialogResult.OK || loginForm.UsuarioAutenticado == null)
                {
                    break;
                }

                tenant.Iniciar(loginForm.UsuarioAutenticado);

                var mainForm = new MainForm(servicos, loginForm.UsuarioAutenticado);
                Application.Run(mainForm);

                bool voltarParaLogin = mainForm.SolicitouLogout;
                tenant.Encerrar();

                if (!voltarParaLogin)
                {
                    break;
                }
            }
        }
    }
}
