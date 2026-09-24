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
    public class ProdutosControl : UserControl
    {
        private readonly ProdutoService _service = new();
        private readonly TextBox _txtBusca = new();
        private readonly DataGridView _grid = new();
        private List<Produto> _produtos = new();

        public ProdutosControl()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Produtos",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            _txtBusca.Location = new Point(25, 60);
            _txtBusca.Size = new Size(300, 28);
            _txtBusca.PlaceholderText = "Pesquisar por nome ou código...";
            _txtBusca.TextChanged += (s, e) => Carregar();
            Controls.Add(_txtBusca);

            var btnNovo = new Button
            {
                Text = "+ Novo Produto",
                Location = new Point(345, 58),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNovo.FlatAppearance.BorderSize = 0;
            btnNovo.Click += (s, e) => AbrirEdicao(null);
            Controls.Add(btnNovo);

            var btnEditar = new Button
            {
                Text = "Editar",
                Location = new Point(505, 58),
                Size = new Size(100, 32)
            };
            btnEditar.Click += (s, e) => EditarSelecionado();
            Controls.Add(btnEditar);

            var btnExcluir = new Button
            {
                Text = "Excluir",
                Location = new Point(615, 58),
                Size = new Size(100, 32)
            };
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
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) EditarSelecionado();
            };
            _grid.CellFormatting += Grid_CellFormatting;
            Controls.Add(_grid);

            UiHelper.EstilizarGrid(_grid);

            Load += (s, e) => Carregar();
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;

            var row = _grid.Rows[e.RowIndex];
            if (row.Cells["Id"].Value is int produtoId)
            {
                var produto = _produtos.FirstOrDefault(p => p.Id == produtoId);
                if (produto != null && e.CellStyle != null)
                {
                    if (produto.Quantidade <= 0)
                        e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                    else if (produto.Quantidade <= produto.EstoqueMinimo)
                        e.CellStyle.ForeColor = Color.FromArgb(230, 126, 34);
                }
            }
        }

        public void Carregar()
        {
            try
            {
                _produtos = _service.GetAll(_txtBusca.Text);

                _grid.DataSource = _produtos.Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Nome,
                    Categoria = p.Categoria?.Nome ?? "-",
                    Fornecedor = p.Fornecedor?.Nome ?? "-",
                    Quantidade = p.Quantidade,
                    Minimo = p.EstoqueMinimo,
                    PrecoVenda = FormatHelper.Moeda(p.PrecoVenda),
                    Situacao = p.SituacaoEstoque
                }).ToList();

                // Oculta a coluna de ID do usuário final
                if (_grid.Columns["Id"] != null)
            // CORRETO
if (_grid.Columns["Nome"] is DataGridViewColumn col)
{
    col.HeaderText = "Nome do Produto";
}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar lista de produtos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Produto? ObterSelecionado()
        {
            if (_grid.CurrentRow == null) return null;

            if (_grid.CurrentRow.Cells["Id"].Value is int id)
            {
                return _produtos.FirstOrDefault(p => p.Id == id);
            }

            return null;
        }

        private void EditarSelecionado()
        {
            var produto = ObterSelecionado();
            if (produto == null)
            {
                MessageBox.Show("Selecione um produto na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AbrirEdicao(produto);
        }

        private void AbrirEdicao(Produto? produto)
        {
            using var form = new ProdutoEditForm(produto?.Id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                Carregar();
            }
        }

        private void ExcluirSelecionado()
        {
            var produto = ObterSelecionado();
            if (produto == null)
            {
                MessageBox.Show("Selecione um produto na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmar = MessageBox.Show(
                $"Deseja realmente excluir o produto \"{produto.Nome}\"?\nEsta ação também remove o histórico de movimentações dele.",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmar == DialogResult.Yes)
            {
                try
                {
                    _service.Delete(produto.Id);
                    Carregar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir o produto: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}