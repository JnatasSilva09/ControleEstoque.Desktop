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
    public class CategoriasControl : UserControl
    {
        private readonly CategoriaService _service;
        private readonly TextBox _txtBusca = new();
        private readonly DataGridView _grid = new();
        private List<CategoriaViewModel> _categorias = new();

        private readonly Servicos _servicos;

        public CategoriasControl(Servicos servicos)
        {
            _servicos = servicos;
            _service = servicos.Categorias;
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
{
    Text = "Categorias",
    Font = UITheme.FonteTitulo,
    ForeColor = UITheme.CorTextoPrincipal,
    AutoSize = true,
    Location = new Point(25, 15)
};
            Controls.Add(titulo);

            _txtBusca.Location = new Point(25, 60);
            _txtBusca.Size = new Size(300, 28);
            _txtBusca.PlaceholderText = "Pesquisar categoria...";
            _txtBusca.TextChanged += (s, e) => Carregar();
            Controls.Add(_txtBusca);

            var btnNovo = new Button
            {
                Text = "+ Nova Categoria",
                Location = new Point(345, 58),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNovo.FlatAppearance.BorderSize = 0;
                      btnNovo.Click += (s, e) => AbrirEdicao(null);
            Controls.Add(btnNovo);

            var btnEditar = new Button { Text = "Editar", Location = new Point(515, 58), Size = new Size(100, 32) };
            btnEditar.Click += (s, e) => EditarSelecionado();
            Controls.Add(btnEditar);

            var btnExcluir = new Button { Text = "Excluir", Location = new Point(625, 58), Size = new Size(100, 32) };
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
            _grid.CellDoubleClick += (s, e) => EditarSelecionado();
            Controls.Add(_grid);
            UiHelper.EstilizarGrid(_grid);

            Load += (s, e) => Carregar();
        }

        public void Carregar()
        {
       _categorias = _service.ObterTodasComContagem(_txtBusca.Text);

    _grid.DataSource = _categorias.Select(c => new
    {
        c.Id,
        c.Nome,
        Descricao = c.Descricao ?? "-",
        QtdProdutos = c.QuantidadeProdutos
    }).ToList();

            // Formatação do cabeçalho das colunas do DataGridView
            if (_grid.Columns["Nome"] != null)
                _grid.Columns["Nome"].HeaderText = "Nome";

            if (_grid.Columns["Descrição"] != null)
                _grid.Columns["Descrição"].HeaderText = "Descrição";

            if (_grid.Columns["QtdProdutos"] != null)
                _grid.Columns["QtdProdutos"].HeaderText = "Qtd. Produtos";

            foreach (DataGridViewColumn coluna in _grid.Columns)
                coluna.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

       private CategoriaViewModel? ObterSelecionado()
        {
            if (_grid.CurrentRow == null) return null;
            var index = _grid.CurrentRow.Index;
            if (index < 0 || index >= _categorias.Count) return null;
            return _categorias[index];
        }

        private void EditarSelecionado()
        {
            var categoria = ObterSelecionado();
            if (categoria == null)
            {
                MessageBox.Show("Selecione uma categoria na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AbrirEdicao(categoria);
        }

      private void AbrirEdicao(CategoriaViewModel? categoria)
        {
            using var form = new CategoriaEditForm(_servicos, categoria?.Id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                Carregar();
            }
        }

        private void ExcluirSelecionado()
        {
            var categoria = ObterSelecionado();
            if (categoria == null)
            {
                MessageBox.Show("Selecione uma categoria na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirmar = MessageBox.Show(
                $"Deseja realmente excluir a categoria \"{categoria.Nome}\"?\nOs produtos vinculados a ela ficarão sem categoria.",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmar == DialogResult.Yes)
            {
                try
                {
                    _service.Delete(categoria.Id);
                    Carregar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao excluir a categoria: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
