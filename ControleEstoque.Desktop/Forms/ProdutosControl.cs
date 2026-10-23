using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public partial class ProdutosControl : UserControl
    {
        private readonly ProdutoService _produtoService = new();
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtBusca = new();
        private readonly Button _btnNovo = new();

        public ProdutosControl()
        {
            // Configurações principais do Controlo
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            // 1. Título do Ecrã
            var lblTitulo = new Label
            {
                Text = "Gestão de Produtos",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 40
            };

            // 2. Barra de Ações / Pesquisa Superior
            var pnlAcoes = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 10),
                BackColor = Color.Transparent
            };

            _txtBusca.Font = new Font("Segoe UI", 10);
            _txtBusca.Size = new Size(260, 30);
            _txtBusca.Location = new Point(0, 15);
            _txtBusca.PlaceholderText = "Pesquisar produto por nome...";
            _txtBusca.TextChanged += (s, e) => CarregarProdutos(_txtBusca.Text);

            _btnNovo.Text = "+ Novo Produto";
            _btnNovo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _btnNovo.BackColor = Color.FromArgb(16, 185, 129); // Verde Destaque de Sucesso
            _btnNovo.ForeColor = Color.White;
            _btnNovo.FlatStyle = FlatStyle.Flat;
            _btnNovo.FlatAppearance.BorderSize = 0;
            _btnNovo.Size = new Size(130, 32);
            _btnNovo.Location = new Point(275, 14);
            _btnNovo.Cursor = Cursors.Hand;
            _btnNovo.Click += (s, e) => {
              var form = new ProdutoEditForm(0);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    CarregarProdutos();
                }
            };

            pnlAcoes.Controls.Add(_txtBusca);
            pnlAcoes.Controls.Add(_btnNovo);

            // 3. Configuração da Grelha de Dados (DataGridView)
            ConfigurarGrid();

            // Adicionar componentes ao UserControl (ordem inversa do Dock)
            Controls.Add(_grid);
            Controls.Add(pnlAcoes);
            Controls.Add(lblTitulo);

            CarregarProdutos();
        }

        private void ConfigurarGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.None;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AllowUserToAddRows = false;
            _grid.ReadOnly = true;
            _grid.EnableHeadersVisualStyles = false;

            // Cabeçalho Corporativo (Azul Escuro)
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _grid.ColumnHeadersHeight = 35;

            // Linhas e Seleção
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            _grid.RowTemplate.Height = 30;
        }

       public void CarregarProdutos(string? filtro = null)
        {
            try
            {
                var lista = _produtoService.GetAll();
                
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    lista = lista.Where(p => p.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                _grid.DataSource = lista;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}