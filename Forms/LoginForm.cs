using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class LoginForm : Form
    {
        private readonly UsuarioService _service = new();

        private readonly TextBox _txtLogin = new();
        private readonly TextBox _txtSenha = new() { PasswordChar = '●' };
        private readonly Label _lblErro = new();

        public Usuario? UsuarioAutenticado { get; private set; }

        public LoginForm()
        {
            Text = "Controle de Estoque — Login";
            Width = 380;
            Height = 330;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            var titulo = new Label
            {
                Text = "CONTROLE DE ESTOQUE",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 25),
                Size = new Size(380, 35)
            };
            Controls.Add(titulo);

            var lblLogin = new Label { Text = "Usuário", Location = new Point(40, 85), AutoSize = true };
            Controls.Add(lblLogin);

            _txtLogin.Location = new Point(40, 107);
            _txtLogin.Size = new Size(300, 26);
            Controls.Add(_txtLogin);

            var lblSenha = new Label { Text = "Senha", Location = new Point(40, 145), AutoSize = true };
            Controls.Add(lblSenha);

            _txtSenha.Location = new Point(40, 167);
            _txtSenha.Size = new Size(300, 26);
            Controls.Add(_txtSenha);

            _lblErro.Location = new Point(40, 198);
            _lblErro.Size = new Size(300, 20);
            _lblErro.ForeColor = Color.FromArgb(192, 57, 43);
            _lblErro.Font = new Font("Segoe UI", 8.5f);
            Controls.Add(_lblErro);

            var btnEntrar = new Button
            {
                Text = "Entrar",
                Location = new Point(40, 225),
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnEntrar.FlatAppearance.BorderSize = 0;
            btnEntrar.Click += BtnEntrar_Click;
            Controls.Add(btnEntrar);

            var btnSair = new Button
            {
                Text = "Sair",
                Location = new Point(200, 225),
                Size = new Size(140, 36)
            };
            btnSair.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnSair);

            // Atalhos de teclado
            AcceptButton = btnEntrar;
            CancelButton = btnSair;

            ActiveControl = _txtLogin;
        }

        private void BtnEntrar_Click(object? sender, EventArgs e)
        {
            _lblErro.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(_txtLogin.Text) || string.IsNullOrWhiteSpace(_txtSenha.Text))
            {
                _lblErro.Text = "Informe usuário e senha.";
                return;
            }

            try
            {
                var usuario = _service.Autenticar(_txtLogin.Text.Trim(), _txtSenha.Text);

                if (usuario == null)
                {
                    _lblErro.Text = "Usuário ou senha inválidos.";
                    _txtSenha.Clear();
                    _txtSenha.Focus();
                    return;
                }

                UsuarioAutenticado = usuario;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _lblErro.Text = "Erro ao conectar ao banco de dados.";
                MessageBox.Show($"Detalhes do erro: {ex.Message}", "Erro de Autenticação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}