using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class RelatoriosControl : UserControl
    {
        private readonly ProdutoService _produtoService = new();
        private readonly MovimentacaoService _movimentacaoService = new();
        private readonly ComboBox _cboRelatorio = new();
        private readonly DataGridView _grid = new();
        private readonly Label _lblTotal = new();

        public RelatoriosControl()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Relatórios",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            var lblEscolha = new Label { Text = "Escolha o relatório:", Location = new Point(25, 60), AutoSize = true };
            Controls.Add(lblEscolha);

            _cboRelatorio.Location = new Point(25, 82);
            _cboRelatorio.Size = new Size(320, 28);
            _cboRelatorio.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboRelatorio.Items.AddRange(new object[]
            {
                "Lista de Produtos",
                "Estoque Atual",
                "Produtos sem Estoque",
                "Produtos com Estoque Baixo",
                "Movimentações — Entradas",
                "Movimentações — Saídas"
            });
            _cboRelatorio.SelectedIndex = 0;
            _cboRelatorio.SelectedIndexChanged += (s, e) => GerarRelatorio();
            Controls.Add(_cboRelatorio);

            var aviso = new Label
            {
                Text = "A exportação para PDF/Excel está planejada para uma fase futura do sistema.",
                Location = new Point(360, 88),
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic)
            };
            Controls.Add(aviso);

            _grid.Location = new Point(25, 125);
            _grid.Size = new Size(1080, 440);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Controls.Add(_grid);
            UiHelper.EstilizarGrid(_grid);

            _lblTotal.Location = new Point(25, 575);
            _lblTotal.AutoSize = true;
            _lblTotal.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Controls.Add(_lblTotal);

            Load += (s, e) => GerarRelatorio();
        }

        private void GerarRelatorio()
        {
            try
            {
                switch (_cboRelatorio.SelectedIndex)
                {
                    case 0: // Lista de produtos
                        var todos = _produtoService.GetAll();
                        _grid.DataSource = todos.Select(p => new
                        {
                            Código = p.Codigo,
                            Nome = p.Nome,
                            Categoria = p.Categoria?.Nome ?? "-",
                            Quantidade = p.Quantidade,
                            Preço = FormatHelper.Moeda(p.PrecoVenda)
                        }).ToList();
                        _lblTotal.Text = $"Total: {todos.Count} produto(s)";
                        break;

                    case 1: // Estoque atual (com valor)
                        var estoque = _produtoService.GetAll();
                        _grid.DataSource = estoque.Select(p => new
                        {
                            Nome = p.Nome,
                            Quantidade = p.Quantidade,
                            CustoUnitario = FormatHelper.Moeda(p.PrecoCusto),
                            ValorTotal = FormatHelper.Moeda(p.PrecoCusto * p.Quantidade)
                        }).ToList();
                        AjustarCabecalhosEstoque();
                        _lblTotal.Text = $"Valor total aproximado do estoque: {FormatHelper.Moeda(_produtoService.ValorTotalEstoque())}";
                        break;

                    case 2: // Sem estoque
                        var semEstoque = _produtoService.GetSemEstoque();
                        _grid.DataSource = semEstoque.Select(p => new
                        {
                            Código = p.Codigo,
                            Nome = p.Nome,
                            EstoqueMínimo = p.EstoqueMinimo
                        }).ToList();
                        _lblTotal.Text = $"Total: {semEstoque.Count} produto(s) sem estoque";
                        break;

                    case 3: // Estoque baixo
                        var estoqueBaixo = _produtoService.GetEstoqueBaixo();
                        _grid.DataSource = estoqueBaixo.Select(p => new
                        {
                            Código = p.Codigo,
                            Nome = p.Nome,
                            Quantidade = p.Quantidade,
                            EstoqueMínimo = p.EstoqueMinimo
                        }).ToList();
                        _lblTotal.Text = $"Total: {estoqueBaixo.Count} produto(s) com estoque baixo";
                        break;

                    case 4: // Entradas
                        var entradas = _movimentacaoService.GetHistorico()
                            .Where(m => m.Tipo == TipoMovimentacao.Entrada)
                            .ToList();

                        _grid.DataSource = entradas.Select(m => new
                        {
                            Data = FormatHelper.DataCurta(m.Data),
                            Produto = m.Produto?.Nome ?? "-",
                            Quantidade = m.Quantidade,
                            Fornecedor = m.FornecedorNome ?? "-"
                        }).ToList();
                        _lblTotal.Text = $"Total de entradas: {entradas.Sum(m => m.Quantidade)} unidade(s)";
                        break;

                    case 5: // Saídas
                        var saidas = _movimentacaoService.GetHistorico()
                            .Where(m => m.Tipo == TipoMovimentacao.Saida)
                            .ToList();

                        _grid.DataSource = saidas.Select(m => new
                        {
                            Data = FormatHelper.DataCurta(m.Data),
                            Produto = m.Produto?.Nome ?? "-",
                            Quantidade = m.Quantidade,
                            Motivo = m.Motivo ?? "-"
                        }).ToList();
                        _lblTotal.Text = $"Total de saídas: {saidas.Sum(m => m.Quantidade)} unidade(s)";
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar o relatório: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

      private void AjustarCabecalhosEstoque()
{
    if (_grid.Columns["CustoUnitario"] is DataGridViewColumn colCusto)
    {
        colCusto.HeaderText = "Custo Unitário";
    }

    if (_grid.Columns["ValorTotal"] is DataGridViewColumn colTotal)
    {
        colTotal.HeaderText = "Valor Total";
         }
}
}
}