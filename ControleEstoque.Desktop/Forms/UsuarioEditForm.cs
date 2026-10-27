using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Dominio.Models;

namespace ControleEstoque.Desktop.Forms
{
    public class UsuarioEditForm : Form
    {
        private readonly UsuarioService _service;
        private readonly int? _usuarioId;

        private readonly TextBox _txtNome = new();
        private readonly TextBox _txtLogin = new();
        private readonly ComboBox _cboPerfil = new();
        private readonly CheckBox _chkAtivo = new() { Text = "Usuário ativo", Checked = true };
        private readonly TextBox _txtSenha = new() { PasswordChar = '●' };
        private readonly TextBox _txtConfirmarSenha = new() { PasswordChar = '●' };
        private readonly Label _lblSenha = new();

        public UsuarioEditForm(Servicos servicos, int? usuarioId)
        {
            _service = servicos.Usuarios;
            _usuarioId = usuarioId;

            Text = usuarioId == null ? "Novo Usuário" : "Editar Usuário";
            Width = 440;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);
            AutoScroll = true;

            var y = 20;

            var lblNome = new Label { Text = "Nome *", Location = new Point(20, y), AutoSize = true };
            Controls.Add(lblNome);
            y += 20;
            _txtNome.Location = new Point(20, y);
            _txtNome.Size = new Size(380, 26);
            Controls.Add(_txtNome);
            y += 46;

            var lblLogin = new Label { Text = "Login *", Location = new Point(20, y), AutoSize = true };
            Controls.Add(lblLogin);
            y += 20;
            _txtLogin.Location = new Point(20, y);
            _txtLogin.Size = new Size(380, 26);
            Controls.Add(_txtLogin);
            y += 46;

            var lblPerfil = new Label { Text = "Perfil", Location = new Point(20, y), AutoSize = true };
            Controls.Add(lblPerfil);
            y += 20;
            _cboPerfil.Location = new Point(20, y);
            _cboPerfil.Size = new Size(220, 26);
            _cboPerfil.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPerfil.Items.Add(PerfilUsuario.Administrador);
            _cboPerfil.Items.Add(PerfilUsuario.Operador);
            _cboPerfil.SelectedIndex = 1; // Operador por padrão
            Controls.Add(_cboPerfil);
            y += 40;

            _chkAtivo.Location = new Point(20, y);
            _chkAtivo.AutoSize = true;
            Controls.Add(_chkAtivo);
            y += 40;

            // Linha divisória visual antes da parte de senha
            var linha = new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(20, y), Size = new Size(380, 2) };
            Controls.Add(linha);
            y += 15;

            _lblSenha.Text = usuarioId == null ? "Senha *" : "Nova Senha (deixe em branco para não alterar)";
            _lblSenha.Location = new Point(20, y);
            _lblSenha.AutoSize = true;
            Controls.Add(_lblSenha);
            y += 20;
            _txtSenha.Location = new Point(20, y);
            _txtSenha.Size = new Size(380, 26);
            Controls.Add(_txtSenha);
            y += 46;

            var lblConfirmar = new Label { Text = "Confirmar Senha", Location = new Point(20, y), AutoSize = true };
            Controls.Add(lblConfirmar);
            y += 20;
            _txtConfirmarSenha.Location = new Point(20, y);
            _txtConfirmarSenha.Size = new Size(380, 26);
            Controls.Add(_txtConfirmarSenha);
            y += 50;

            var btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(200, y),
                Size = new Size(100, 34),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            Controls.Add(btnSalvar);

            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(305, y), Size = new Size(95, 34) };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancelar);

            // Suporte para atalhos do teclado
            AcceptButton = btnSalvar;
            CancelButton = btnCancelar;

            Height = y + 90;

            CarregarExistente();
        }

        private void CarregarExistente()
        {
            if (_usuarioId == null) return;

            try
            {
                var usuario = _service.GetById(_usuarioId.Value);
                if (usuario == null)
                {
                    MessageBox.Show("Usuário não encontrado.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                _txtNome.Text = usuario.Nome;
                _txtLogin.Text = usuario.Login;
                _cboPerfil.SelectedItem = usuario.Perfil;
                _chkAtivo.Checked = usuario.Ativo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados do usuário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtNome.Text) || string.IsNullOrWhiteSpace(_txtLogin.Text))
            {
                MessageBox.Show("Os campos Nome e Login são obrigatórios.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var perfil = (PerfilUsuario)(_cboPerfil.SelectedItem ?? PerfilUsuario.Operador);

            var informouSenha = !string.IsNullOrWhiteSpace(_txtSenha.Text);

            if (_usuarioId == null && !informouSenha)
            {
                MessageBox.Show("Informe uma senha para o novo usuário.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (informouSenha && _txtSenha.Text != _txtConfirmarSenha.Text)
            {
                MessageBox.Show("As senhas digitadas não coincidem.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_usuarioId == null)
                {
                    _service.Add(_txtNome.Text.Trim(), _txtLogin.Text.Trim(), _txtSenha.Text, perfil);
                }
                else
                {
                    _service.AtualizarDadosBasicos(_usuarioId.Value, _txtNome.Text.Trim(), _txtLogin.Text.Trim(), perfil, _chkAtivo.Checked);

                    if (informouSenha)
                    {
                        _service.RedefinirSenha(_usuarioId.Value, _txtSenha.Text);
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Não foi possível salvar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado ao salvar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}