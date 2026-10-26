using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;

namespace ControleEstoque.Desktop.Forms
{
    // Aparece no login quando o usuário precisa criar uma senha própria
    // (por exemplo, depois que o administrador cadastrou ou redefiniu a senha dele).
    public class TrocarSenhaForm : Form
    {
        private readonly TextBox _txtNovaSenha = new() { PasswordChar = '●' };
        private readonly TextBox _txtConfirmarSenha = new() { PasswordChar = '●' };
        private readonly Label _lblErro = new();

        public string NovaSenha => _txtNovaSenha.Text;

        public TrocarSenhaForm()
        {
            Text = "Criar nova senha";
            Width = 380;
            Height = 340;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            var lblAviso = new Label
            {
                Text = "Por segurança, crie uma senha nova antes de continuar.\nUse 8 ou mais caracteres, com maiúscula, minúscula, número e símbolo.",
                Location = new Point(30, 20),
                Size = new Size(310, 55)
            };
            Controls.Add(lblAviso);

            var lblNova = new Label { Text = "Nova senha", Location = new Point(30, 85), AutoSize = true };
            Controls.Add(lblNova);
            _txtNovaSenha.Location = new Point(30, 107);
            _txtNovaSenha.Size = new Size(300, 26);
            Controls.Add(_txtNovaSenha);

            var lblConfirmar = new Label { Text = "Confirmar nova senha", Location = new Point(30, 145), AutoSize = true };
            Controls.Add(lblConfirmar);
            _txtConfirmarSenha.Location = new Point(30, 167);
            _txtConfirmarSenha.Size = new Size(300, 26);
            Controls.Add(_txtConfirmarSenha);

            _lblErro.Location = new Point(30, 198);
            _lblErro.Size = new Size(300, 36);
            _lblErro.ForeColor = Color.FromArgb(192, 57, 43);
            _lblErro.Font = new Font("Segoe UI", 8.5f);
            Controls.Add(_lblErro);

            var btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(30, 245),
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            Controls.Add(btnSalvar);

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(190, 245),
                Size = new Size(140, 36)
            };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancelar);

            AcceptButton = btnSalvar;
            CancelButton = btnCancelar;
            ActiveControl = _txtNovaSenha;
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            var (senhaValida, mensagem) = PasswordHelper.ValidatePasswordStrength(_txtNovaSenha.Text);
            if (!senhaValida)
            {
                _lblErro.Text = mensagem;
                return;
            }

            if (_txtNovaSenha.Text != _txtConfirmarSenha.Text)
            {
                _lblErro.Text = "As senhas digitadas não coincidem.";
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
