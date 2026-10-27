using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class LoginForm : Form
    {
    private readonly AuthService _service;

        private readonly TextBox _txtLogin = new();
        private readonly TextBox _txtSenha = new() { PasswordChar = '●' };
        private readonly Label _lblErro = new();

        // Proteção simples contra tentativas seguidas de adivinhar a senha
        private const int MaximoTentativas = 5;
        private const int SegundosDeEspera = 30;
        private int _tentativasInvalidas;
        private DateTime _bloqueadoAte = DateTime.MinValue;

        public Usuario? UsuarioAutenticado { get; private set; }

        private readonly Servicos _servicos;

        public LoginForm(Servicos servicos)
        {
            _servicos = servicos;
            _service = servicos.Autenticacao;
            Text = "Controle de Estoque — Login";
            Width = 380;
            Height = 370;
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
var btnCriarConta = new Button
{
    Text = "Criar Nova Conta / Teste Grátis",
    Location = new Point(40, 270),
    Size = new Size(300, 32),
    BackColor = Color.FromArgb(245, 247, 250),
    ForeColor = Color.FromArgb(37, 99, 235), // Tom azul corporativo
    FlatStyle = FlatStyle.Flat,
    Cursor = Cursors.Hand
};
btnCriarConta.FlatAppearance.BorderSize = 1;
btnCriarConta.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
btnCriarConta.Click += (s, e) =>
{
    var registerForm = new RegisterForm(_servicos);
    registerForm.ShowDialog();
};
Controls.Add(btnCriarConta);
            // Atalhos de teclado
            AcceptButton = btnEntrar;
            CancelButton = btnSair;

            ActiveControl = _txtLogin;
        }

        private void BtnEntrar_Click(object? sender, EventArgs e)
        {
            _lblErro.Text = string.Empty;

            if (DateTime.Now < _bloqueadoAte)
            {
                var segundos = (int)Math.Ceiling((_bloqueadoAte - DateTime.Now).TotalSeconds);
                _lblErro.Text = $"Muitas tentativas. Aguarde {segundos}s.";
                return;
            }

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
                    _tentativasInvalidas++;
                    if (_tentativasInvalidas >= MaximoTentativas)
                    {
                        _tentativasInvalidas = 0;
                        _bloqueadoAte = DateTime.Now.AddSeconds(SegundosDeEspera);
                        _lblErro.Text = $"Muitas tentativas. Aguarde {SegundosDeEspera}s.";
                    }
                    else
                    {
                        _lblErro.Text = "Usuário ou senha inválidos.";
                    }

                    _txtSenha.Clear();
                    _txtSenha.Focus();
                    return;
                }

                _tentativasInvalidas = 0;

                // Verifica se o período de teste/assinatura expirou
                if (usuario.Empresa != null)
                {
                    if (usuario.Empresa.StatusAssinatura == StatusAssinatura.Bloqueada)
                    {
                        MessageBox.Show("O acesso desta empresa está bloqueado.\nPor favor, entre em contato com o suporte.",
                                        "Acesso Bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (usuario.Empresa.StatusAssinatura == StatusAssinatura.Degustacao && DateTime.Now > usuario.Empresa.DataVencimento)
                    {
                        MessageBox.Show("O seu período de teste grátis de 14 dias expirou.\nPor favor, entre em contato com o suporte ou atualize a sua assinatura para continuar.",
                                        "Teste Expirado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Senha provisória (usuário novo ou senha redefinida): precisa criar uma senha própria
                if (usuario.PrecisaTrocarSenha)
                {
                    using var trocarSenha = new TrocarSenhaForm();
                    if (trocarSenha.ShowDialog(this) != DialogResult.OK)
                    {
                        _lblErro.Text = "Crie uma nova senha para continuar.";
                        _txtSenha.Clear();
                        return;
                    }

                    _service.AlterarSenha(usuario.Id, trocarSenha.NovaSenha);
                }

                // Se tudo estiver ok, prossegue para o Dashboard principal
                UsuarioAutenticado = usuario;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Login");
                _lblErro.Text = "Erro ao conectar ao banco de dados.";
                MessageBox.Show($"Detalhes do erro: {ex.Message}", "Erro de Autenticação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
