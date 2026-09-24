using System;
using System.IO;
using System.Windows.Forms;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Captura erros de formulários/telas (Thread de UI)
            Application.ThreadException += (sender, e) => TratarExcecaoGlobal(e.Exception);

            // Captura erros fora da interface gráfica (Tasks, Threads de fundo)
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => TratarExcecaoGlobal(e.ExceptionObject as Exception);

            try
            {
                IniciarSistema();
            }
            catch (Exception ex)
            {
                TratarExcecaoGlobal(ex);
            }
        }

        private static void IniciarSistema()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            new UsuarioService().GarantirUsuarioAdminPadrao();

            while (true)
            {
                Usuario? usuarioLogado;

                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                        break;

                    usuarioLogado = loginForm.UsuarioAutenticado;
                }

                if (usuarioLogado == null) break;

                using var mainForm = new MainForm(usuarioLogado);
                Application.Run(mainForm);

                if (!mainForm.SolicitouLogout) break;
            }
        }

        private static void TratarExcecaoGlobal(Exception? ex)
        {
            if (ex == null) return;

            var mensagem = ex.Message;
            if (ex.InnerException != null)
            {
                mensagem += $"\n\nDetalhes adicionais: {ex.InnerException.Message}";
            }

            MessageBox.Show(
                $"Ocorreu um erro inesperado no sistema:\n\n{mensagem}",
                "Erro no Sistema",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}