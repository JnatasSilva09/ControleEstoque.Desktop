using System;
using System.Windows.Forms;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Forms; // Importa o namespace onde estão os teus Forms

namespace ControleEstoque.Desktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Garante que a base de dados e todas as tabelas (incluindo Empresas) são criadas automaticamente
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
            }

            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.UsuarioAutenticado != null)
            {
                Application.Run(new MainForm(loginForm.UsuarioAutenticado));
           }
        }
    }
}
        