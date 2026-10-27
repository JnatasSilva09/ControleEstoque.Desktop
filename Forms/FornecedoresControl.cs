using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class FornecedoresControl : UserControl
    {
        private readonly FornecedorService _service;
        private readonly TextBox _txtBusca = new();
        private readonly DataGridView _grid = new();
        private List<Fornecedor> _fornecedores = new();

        private readonly Servicos _servicos;

        public FornecedoresControl(Servicos servicos)
        {
            _servicos = servicos;
            _service = servicos.Fornecedores;
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Fornecedores",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            _txtBusca.Location = new Point(25, 60);
            _txtBusca.Size = new Size(300, 28);
            _txtBusca.PlaceholderText = "Pesquisar fornecedor...";
            _txtBusca.TextChanged += (s, e) => Carregar();
            Controls.Add(_txtBusca);

            var btnNovo = new Button
            {
                Text = "+ Novo Fornecedor",
                Location = new Point(345, 58),
                Size = new Size(165, 32),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNovo.FlatAppearance.BorderSize = 0;
            btnNovo.Click += (s, e) => AbrirEdicao(null);
            Controls.Add(btnNovo);

            var btnEditar = new Button { Text = "Editar", Location = new Point(520, 58), Size = new Size(100, 32) };
            btnEditar.Click += (s, e) => EditarSelecionado();
            Controls.Add(btnEditar);

            var btnExcluir = new Button { Text = "Excluir", Location = new Point(630, 58), Size = new Size(100, 32) };
            btnExcluir.Click += (s, e) => ExcluirSelecionado();
            Controls.Add(btnExcluir);

            _grid.Location = new Point(25, 105);
            _grid.Size = new Size(1080, 480);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.CellDoubleClick += Grid_CellDoubleClick;
            Controls.Add(_grid);

            UiHelper.EstilizarGrid(_grid);

            Load += (s, e) => Carregar();
        }

        public void Carregar()
        {
            try
            {
                _fornecedores = _service.GetAll(_txtBusca.Text.Trim());

                _grid.DataSource = _fornecedores.Select(f => new
                {
                    f.Id,
                    f.Nome,
                    CpfCnpj = string.IsNullOrWhiteSpace(f.CpfCnpj) ? "-" : f.CpfCnpj,
                    Telefone = string.IsNullOrWhiteSpace(f.Telefone) ? "-" : f.Telefone,
                    WhatsApp = string.IsNullOrWhiteSpace(f.WhatsApp) ? "-" : f.WhatsApp,
                    Email = string.IsNullOrWhiteSpace(f.Email) ? "-" : f.Email
                }).ToList();

                AjustarCabecalhosGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar lista de fornecedores: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       private void AjustarCabecalhosGrid()
{
    if (_grid.Columns["Id"] is DataGridViewColumn colId)
        colId.Visible = false;

    if (_grid.Columns["Nome"] is DataGridViewColumn colNome)
        colNome.HeaderText = "Nome / Razão Social";

    if (_grid.Columns["CpfCnpj"] is DataGridViewColumn colCpfCnpj)
        colCpfCnpj.HeaderText = "CPF / CNPJ";

    if (_grid.Columns["Telefone"] is DataGridViewColumn colTelefone)
        colTelefone.HeaderText = "Telefone";

    if (_grid.Columns["WhatsApp"] is DataGridViewColumn colWhatsApp)
        colWhatsApp.HeaderText = "WhatsApp";

    if (_grid.Columns["Email"] is DataGridViewColumn colEmail)
        colEmail.HeaderText = "E-mail";

    foreach (DataGridViewColumn coluna in _grid.Columns)
    {
        coluna.SortMode = DataGridViewColumnSortMode.NotSortable;
    }
}

        private Fornecedor? ObterSelecionado()
        {
            if (_grid.CurrentRow == null) return null;
            var index = _grid.CurrentRow.Index;

            if (index < 0 || index >= _fornecedores.Count) return null;
            return _fornecedores[index];
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditarSelecionado();
            }
        }

        private void EditarSelecionado()
        {
            var fornecedor = ObterSelecionado();
            if (fornecedor == null)
            {
                MessageBox.Show("Selecione um fornecedor na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AbrirEdicao(fornecedor);
        }

        private void AbrirEdicao(Fornecedor? fornecedor)
        {
            using var form = new FornecedorEditForm(_servicos, fornecedor?.Id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                Carregar();
            }
        }

        private void ExcluirSelecionado()
        {
            var fornecedor = ObterSelecionado();
            if (fornecedor == null)
            {
                MessageBox.Show("Selecione um fornecedor na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmar = MessageBox.Show(
                $"Deseja realmente excluir o fornecedor \"{fornecedor.Nome}\"?\nOs produtos vinculados a ele ficarão sem fornecedor.",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmar == DialogResult.Yes)
            {
                try
                {
                    _service.Delete(fornecedor.Id);
                    Carregar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir fornecedor: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}