using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class CategoriaEditForm : Form
    {
        private readonly CategoriaService _service = new();
        private readonly int? _categoriaId;

        private readonly TextBox _txtNome = new();
        private readonly TextBox _txtDescricao = new() { Multiline = true };

        public CategoriaEditForm(int? categoriaId)
        {
            _categoriaId = categoriaId;

            Text = categoriaId == null ? "Nova Categoria" : "Editar Categoria";
            Width = 420;
            Height = 260;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            var lblNome = new Label { Text = "Nome *", Location = new Point(20, 20), AutoSize = true };
            Controls.Add(lblNome);
            _txtNome.Location = new Point(20, 42);
            _txtNome.Size = new Size(360, 26);
            Controls.Add(_txtNome);

            var lblDescricao = new Label { Text = "Descrição", Location = new Point(20, 80), AutoSize = true };
            Controls.Add(lblDescricao);
            _txtDescricao.Location = new Point(20, 102);
            _txtDescricao.Size = new Size(360, 60);
            Controls.Add(_txtDescricao);

            var btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(180, 175),
                Size = new Size(100, 34),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            Controls.Add(btnSalvar);

            var btnCancelar = new Button { Text = "Cancelar", Location = new Point(285, 175), Size = new Size(95, 34) };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancelar);

            // Permite usar a tecla Enter para salvar e Esc para fechar/cancelar
            AcceptButton = btnSalvar;
            CancelButton = btnCancelar;

            CarregarExistente();
        }

        private void CarregarExistente()
        {
            if (_categoriaId == null) return;

            try
            {
                var categoria = _service.GetById(_categoriaId.Value);
                if (categoria == null)
                {
                    MessageBox.Show("Categoria não encontrada.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                _txtNome.Text = categoria.Nome;
                _txtDescricao.Text = categoria.Descricao;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar categoria: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (_categoriaId == null)
                {
                    _service.Add(new Categoria
                    {
                        Nome = _txtNome.Text.Trim(),
                        Descricao = _txtDescricao.Text.Trim()
                    });
                }
                else
                {
                    var categoria = _service.GetById(_categoriaId.Value);
                    if (categoria == null)
                    {
                        MessageBox.Show("Categoria não encontrada.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    categoria.Nome = _txtNome.Text.Trim();
                    categoria.Descricao = _txtDescricao.Text.Trim();
                    _service.Update(categoria);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                // Captura regras de negócio tratadas no serviço (ex: nome duplicado)
                MessageBox.Show(ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Captura outros erros inesperados
                MessageBox.Show($"Ocorreu um erro ao salvar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
