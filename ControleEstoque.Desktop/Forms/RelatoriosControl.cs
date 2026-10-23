using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public partial class RelatoriosControl : UserControl
    {
        private readonly ProdutoService _produtoService = new();
        private readonly MovimentacaoService _movimentacaoService = new();
        
        private readonly ComboBox _cboRelatorio = new();
        private readonly DataGridView _grid = new();
        private readonly Label _lblTotal = new();

        public RelatoriosControl()
        {
            // Propriedades principais do controlo
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            // Título do Ecrã
            var titulo = new Label
            {
                Text = "Relatórios do Sistema",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 40
            };

            // Configuração da grelha de dados (DataGridView)
            _grid.Dock = DockStyle.Fill;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.None;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Adicionar os controlos ao UserControl (ordem inversa devido ao Dock)
            Controls.Add(_grid);
            Controls.Add(titulo);

            // Carregar os dados iniciais
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                // Podes carregar por exemplo a lista de produtos ou movimentações
                // _grid.DataSource = _produtoService.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar relatório: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}