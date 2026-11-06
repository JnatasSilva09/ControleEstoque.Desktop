using System;
using System.Windows.Forms;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Desktop.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Dominio.Contexto;
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
            var tenant = new ContextoDoTenantManual();
            AppDbContextFactory fabrica;
            Servicos servicos;

            try
            {
                var conexao = ConfiguracaoBanco.ObterConexao();
                fabrica = new AppDbContextFactory(tenant, conexao);
                servicos = new Servicos(fabrica, tenant);

                // Cria o banco e as tabelas se ainda não existirem
                // (na Fase 2 passo 4 isto vira "migrations")
                using var context = fabrica.Criar();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Inicialização do banco de dados");
                MessageBox.Show("Não foi possível conectar ao banco de dados PostgreSQL.\n\n" +
                                "O container está rodando? (docker compose up -d)\n\n" + ex.Message,
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
