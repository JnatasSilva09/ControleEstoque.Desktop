using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AuthService _authService = new();

        private readonly TextBox _txtNomeEmpresa = new() { PlaceholderText = "Nome da sua Empresa" };
        private readonly TextBox _txtEmail = new() { PlaceholderText = "Seu E-mail" };
        private readonly TextBox _txtSenha = new() { PlaceholderText = "Sua Senha", PasswordChar = '*' };
        private readonly Button _btnCadastrar = new();

        public RegisterForm()
        {
            Text = "Criar Conta - Teste Grátis";
            Size = new Size(400, 420);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 246, 248);

            var lblTitulo = new Label
            {
                Text = "Comece seu Teste Grátis",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Estilização dos campos
            ConfigurarCampo(_txtNomeEmpresa, 80);
            ConfigurarCampo(_txtEmail, 130);
            ConfigurarCampo(_txtSenha, 180);

            _btnCadastrar.Text = "Criar Conta e Entrar";
            _btnCadastrar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _btnCadastrar.BackColor = Color.FromArgb(16, 185, 129); // Verde Destaque
            _btnCadastrar.ForeColor = Color.White;
            _btnCadastrar.FlatStyle = FlatStyle.Flat;
            _btnCadastrar.FlatAppearance.BorderSize = 0;
            _btnCadastrar.Size = new Size(300, 40);
            _btnCadastrar.Location = new Point(42, 240);
            _btnCadastrar.Cursor = Cursors.Hand;
            _btnCadastrar.Click += BtnCadastrar_Click;

            Controls.Add(_btnCadastrar);
            Controls.Add(_txtNomeEmpresa);
            Controls.Add(_txtEmail);
            Controls.Add(_txtSenha);
            Controls.Add(lblTitulo);
        }

        private void ConfigurarCampo(TextBox txt, int top)
        {
            txt.Font = new Font("Segoe UI", 10);
            txt.Size = new Size(300, 30);
            txt.Location = new Point(42, top);
        }

       private void BtnCadastrar_Click(object? sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(_txtNomeEmpresa.Text) || 
        string.IsNullOrWhiteSpace(_txtEmail.Text) || 
        string.IsNullOrWhiteSpace(_txtSenha.Text))
    {
        MessageBox.Show("Por favor, preencha todos os campos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // Validação de formato de e-mail básico
    if (!FormatHelper.ValidarEmail(_txtEmail.Text))
    {
        MessageBox.Show("Por favor, informe um endereço de e-mail válido.", "E-mail Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // Validação da força da senha
    var (senhaValida, mensagemErro) = PasswordHelper.ValidatePasswordStrength(_txtSenha.Text);
    if (!senhaValida)
    {
        MessageBox.Show(mensagemErro, "Requisitos de Senha", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    try
    {
        bool sucesso = _authService.RegistrarNovaEmpresa(_txtNomeEmpresa.Text.Trim(), _txtEmail.Text.Trim(), _txtSenha.Text);

        if (sucesso)
        {
            MessageBox.Show("Conta criada com sucesso! Você tem 14 dias de teste grátis.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
  catch (Exception)
    {
        // Em produção, evite mostrar detalhes internos do banco de dados...
        MessageBox.Show("Ocorreu um erro ao criar a conta. Verifique se o e-mail já está cadastrado ou tente novamente.", 
                        "Erro no Cadastro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    } 
}
}
}
