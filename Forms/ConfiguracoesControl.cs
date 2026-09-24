using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class ConfiguracoesControl : UserControl
    {
        private readonly ConfiguracaoService _service = new();

        private readonly TextBox _txtNomeEmpresa = new();
        private readonly TextBox _txtCnpj = new();
        private readonly TextBox _txtTelefone = new();
        private readonly TextBox _txtWhatsApp = new();
        private readonly TextBox _txtEmail = new();
        private readonly TextBox _txtEndereco = new();
        private readonly PictureBox _picLogo = new();
        private byte[]? _logoBytes;
        private int _configId;

        public ConfiguracoesControl()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);
            AutoScroll = true;

            var titulo = new Label
            {
                Text = "Configurações da Empresa",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            var painel = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(560, 480),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(painel);

            var y = 20;
            AdicionarCampo(painel, "Nome da Empresa *", _txtNomeEmpresa, ref y);
            AdicionarCampo(painel, "CNPJ", _txtCnpj, ref y);
            AdicionarCampo(painel, "Telefone", _txtTelefone, ref y);
            AdicionarCampo(painel, "WhatsApp", _txtWhatsApp, ref y);
            AdicionarCampo(painel, "E-mail", _txtEmail, ref y);
            AdicionarCampo(painel, "Endereço", _txtEndereco, ref y);

            var lblLogo = new Label { Text = "Logo", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblLogo);
            y += 20;

            _picLogo.Location = new Point(20, y);
            _picLogo.Size = new Size(100, 100);
            _picLogo.BorderStyle = BorderStyle.FixedSingle;
            _picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            painel.Controls.Add(_picLogo);

            var btnEscolherLogo = new Button
            {
                Text = "Escolher imagem...",
                Location = new Point(135, y),
                Size = new Size(150, 30)
            };
            btnEscolherLogo.Click += BtnEscolherLogo_Click;
            painel.Controls.Add(btnEscolherLogo);

            y += 115;

            var btnSalvar = new Button
            {
                Text = "Salvar Configurações",
                Location = new Point(20, y),
                Size = new Size(200, 38),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            painel.Controls.Add(btnSalvar);

            y += 60;
            painel.Height = y;

            Load += (s, e) => CarregarConfiguracao();
        }

        private void AdicionarCampo(Panel painel, string rotulo, TextBox campo, ref int y)
        {
            var lbl = new Label { Text = rotulo, Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lbl);
            y += 20;
            campo.Location = new Point(20, y);
            campo.Size = new Size(500, 26);
            painel.Controls.Add(campo);
            y += 46;
        }

        private void CarregarConfiguracao()
        {
            try
            {
                var config = _service.GetConfiguracao();
                if (config == null) return;

                _configId = config.Id;
                _txtNomeEmpresa.Text = config.NomeEmpresa;
                _txtCnpj.Text = config.Cnpj;
                _txtTelefone.Text = config.Telefone;
                _txtWhatsApp.Text = config.WhatsApp;
                _txtEmail.Text = config.Email;
                _txtEndereco.Text = config.Endereco;
                _logoBytes = config.Logo;

                ExibirLogo(_logoBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar as configurações: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEscolherLogo_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Imagens (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Selecione a Logo da Empresa"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _logoBytes = File.ReadAllBytes(dialog.FileName);
                    ExibirLogo(_logoBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem selecionada: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExibirLogo(byte[]? bytes)
        {
            if (bytes is { Length: > 0 })
            {
                using var ms = new MemoryStream(bytes);
                var imgAnterior = _picLogo.Image;
                _picLogo.Image = Image.FromStream(ms);
                imgAnterior?.Dispose(); // Libera recurso da imagem anterior da memória
            }
            else
            {
                _picLogo.Image = null;
            }
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtNomeEmpresa.Text))
            {
                MessageBox.Show("O nome da empresa é obrigatório.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNomeEmpresa.Focus();
                return;
            }

            try
            {
                _service.Salvar(new Models.Configuracao
                {
                    Id = _configId,
                    NomeEmpresa = _txtNomeEmpresa.Text.Trim(),
                    Cnpj = _txtCnpj.Text.Trim(),
                    Telefone = _txtTelefone.Text.Trim(),
                    WhatsApp = _txtWhatsApp.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    Endereco = _txtEndereco.Text.Trim(),
                    Logo = _logoBytes
                });

                MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar as configurações: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
