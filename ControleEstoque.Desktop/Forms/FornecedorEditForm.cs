using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class FornecedorEditForm : Form
    {
        private readonly FornecedorService _service;
        private readonly int? _fornecedorId;

        private readonly TextBox _txtNome = new();
        private readonly TextBox _txtCpfCnpj = new();
        private readonly TextBox _txtTelefone = new();
        private readonly TextBox _txtWhatsApp = new();
        private readonly TextBox _txtEmail = new();
        private readonly TextBox _txtEndereco = new();
        private readonly TextBox _txtObservacoes = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };

        public FornecedorEditForm(Servicos servicos, int? fornecedorId)
        {
            _service = servicos.Fornecedores;
            _fornecedorId = fornecedorId;

            Text = fornecedorId == null ? "Novo Fornecedor" : "Editar Fornecedor";
            Width = 460;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            var y = 20;
            AdicionarCampo("Nome *", _txtNome, ref y);
            AdicionarCampo("CNPJ / CPF", _txtCpfCnpj, ref y);
            AdicionarCampo("Telefone", _txtTelefone, ref y);
            AdicionarCampo("WhatsApp", _txtWhatsApp, ref y);
            AdicionarCampo("E-mail", _txtEmail, ref y);
            AdicionarCampo("Endereço", _txtEndereco, ref y);

            var lblObs = new Label { Text = "Observações", Location = new Point(20, y), AutoSize = true };
            Controls.Add(lblObs);
            y += 20;

            _txtObservacoes.Location = new Point(20, y);
            _txtObservacoes.Size = new Size(400, 70);
            Controls.Add(_txtObservacoes);
            y += 80;

            var btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(220, y),
                Size = new Size(100, 34),
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
                Location = new Point(325, y),
                Size = new Size(95, 34)
            };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancelar);

            // Melhora a navegação via teclado (Enter salva, Esc cancela)
            AcceptButton = btnSalvar;
            CancelButton = btnCancelar;

            Height = y + 85;

            CarregarExistente();
        }

        private void AdicionarCampo(string rotulo, TextBox campo, ref int y)
        {
            var lbl = new Label { Text = rotulo, Location = new Point(20, y), AutoSize = true };
            Controls.Add(lbl);
            y += 20;

            campo.Location = new Point(20, y);
            campo.Size = new Size(400, 26);
            Controls.Add(campo);
            y += 46;
        }

        private void CarregarExistente()
        {
            if (_fornecedorId == null) return;

            try
            {
                var fornecedor = _service.GetById(_fornecedorId.Value);
                if (fornecedor == null)
                {
                    MessageBox.Show("Fornecedor não encontrado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                _txtNome.Text = fornecedor.Nome ?? string.Empty;
                _txtCpfCnpj.Text = fornecedor.CpfCnpj ?? string.Empty;
                _txtTelefone.Text = fornecedor.Telefone ?? string.Empty;
                _txtWhatsApp.Text = fornecedor.WhatsApp ?? string.Empty;
                _txtEmail.Text = fornecedor.Email ?? string.Empty;
                _txtEndereco.Text = fornecedor.Endereco ?? string.Empty;
                _txtObservacoes.Text = fornecedor.Observacoes ?? string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados do fornecedor: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSalvar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtNome.Text))
            {
                MessageBox.Show("O campo Nome é obrigatório.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNome.Focus();
                return;
            }

            try
            {
                if (_fornecedorId == null)
                {
                    _service.Add(new Fornecedor
                    {
                        Nome = _txtNome.Text.Trim(),
                        CpfCnpj = _txtCpfCnpj.Text.Trim(),
                        Telefone = _txtTelefone.Text.Trim(),
                        WhatsApp = _txtWhatsApp.Text.Trim(),
                        Email = _txtEmail.Text.Trim(),
                        Endereco = _txtEndereco.Text.Trim(),
                        Observacoes = _txtObservacoes.Text.Trim()
                    });
                }
                else
                {
                    var fornecedor = _service.GetById(_fornecedorId.Value);
                    if (fornecedor == null)
                    {
                        MessageBox.Show("Não foi possível localizar o fornecedor para atualização.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    fornecedor.Nome = _txtNome.Text.Trim();
                    fornecedor.CpfCnpj = _txtCpfCnpj.Text.Trim();
                    fornecedor.Telefone = _txtTelefone.Text.Trim();
                    fornecedor.WhatsApp = _txtWhatsApp.Text.Trim();
                    fornecedor.Email = _txtEmail.Text.Trim();
                    fornecedor.Endereco = _txtEndereco.Text.Trim();
                    fornecedor.Observacoes = _txtObservacoes.Text.Trim();

                    _service.Update(fornecedor);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar fornecedor: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}