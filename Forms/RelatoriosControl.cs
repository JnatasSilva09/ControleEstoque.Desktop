using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    // Relatório de movimentações: filtra por produto e período, mostra um resumo e exporta para CSV.
    public class RelatoriosControl : UserControl
    {
        // Uma linha do relatório. É isso que aparece na tela e também o que vai para o CSV.
        private record LinhaRelatorio(string Data, string Produto, string Tipo, int Quantidade, string CustoUnitario, string Detalhe);

        private readonly MovimentacaoService _movimentacaoService;
        private readonly TextBox _txtProduto = new();
        private readonly DateTimePicker _dtInicio = new();
        private readonly DateTimePicker _dtFim = new();
        private readonly DataGridView _grid = new();
        private readonly Label _lblEntradas = new();
        private readonly Label _lblSaidas = new();
        private readonly Label _lblSaldo = new();
        private readonly Label _lblTotal = new();
        private List<LinhaRelatorio> _linhas = new();

        public RelatoriosControl(Servicos servicos)
        {
            _movimentacaoService = servicos.Movimentacoes;

            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Relatórios",
                Font = UITheme.FonteTitulo,
                ForeColor = UITheme.CorTextoPrincipal,
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            // Filtros
            _txtProduto.Location = new Point(25, 60);
            _txtProduto.Size = new Size(220, 28);
            _txtProduto.PlaceholderText = "Filtrar por produto...";
            Controls.Add(_txtProduto);

            var lblDe = new Label { Text = "De", AutoSize = true, Location = new Point(265, 65) };
            Controls.Add(lblDe);

            _dtInicio.Location = new Point(290, 60);
            _dtInicio.Size = new Size(110, 26);
            _dtInicio.Format = DateTimePickerFormat.Short;
            _dtInicio.Value = DateTime.Now.AddDays(-30);
            Controls.Add(_dtInicio);

            var lblAte = new Label { Text = "até", AutoSize = true, Location = new Point(410, 65) };
            Controls.Add(lblAte);

            _dtFim.Location = new Point(440, 60);
            _dtFim.Size = new Size(110, 26);
            _dtFim.Format = DateTimePickerFormat.Short;
            _dtFim.Value = DateTime.Now;
            Controls.Add(_dtFim);

            var btnExportar = new Button
            {
                Text = "Exportar CSV",
                Location = new Point(575, 58),
                Size = new Size(140, 32),
                BackColor = UiHelper.CorPrimaria,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportar.FlatAppearance.BorderSize = 0;
            btnExportar.Click += BtnExportarCsv_Click;
            Controls.Add(btnExportar);

            // Resumo do período
            Controls.Add(CriarCartao("Entradas (un.)", _lblEntradas, UiHelper.CorSucesso, 25));
            Controls.Add(CriarCartao("Saídas (un.)", _lblSaidas, UiHelper.CorPerigo, 205));
            Controls.Add(CriarCartao("Saldo do período (un.)", _lblSaldo, UiHelper.CorPrimaria, 385));
            Controls.Add(CriarCartao("Movimentações", _lblTotal, UiHelper.CorSidebar, 565));

            // Tabela
            _grid.Location = new Point(25, 190);
            _grid.Size = new Size(1080, 400);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.CellFormatting += Grid_CellFormatting;
            Controls.Add(_grid);
            UiHelper.EstilizarGrid(_grid);

            // Só depois de montar tudo é que os filtros passam a atualizar o relatório
            _txtProduto.TextChanged += (s, e) => Carregar();
            _dtInicio.ValueChanged += (s, e) => Carregar();
            _dtFim.ValueChanged += (s, e) => Carregar();

            Load += (s, e) => Carregar();
        }

        private static Panel CriarCartao(string titulo, Label valor, Color cor, int x)
        {
            var painel = new Panel
            {
                Location = new Point(x, 105),
                Size = new Size(170, 70),
                BackColor = cor
            };

            valor.Text = "0";
            valor.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            valor.ForeColor = Color.White;
            valor.AutoSize = true;
            valor.Location = new Point(10, 8);
            painel.Controls.Add(valor);

            painel.Controls.Add(new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 44)
            });

            return painel;
        }

        private void Carregar()
        {
            try
            {
                if (_dtInicio.Value.Date > _dtFim.Value.Date)
                {
                    // Período invertido: não consulta, só limpa a tela até a pessoa corrigir
                    _linhas = new List<LinhaRelatorio>();
                    _grid.DataSource = _linhas;
                    AtualizarResumo(0, 0, 0);
                    return;
                }

                var inicio = _dtInicio.Value.Date;
                var fim = _dtFim.Value.Date.AddDays(1).AddTicks(-1);

                var movimentacoes = _movimentacaoService.GetHistorico(_txtProduto.Text, inicio, fim);

                _linhas = movimentacoes.Select(m => new LinhaRelatorio(
                    FormatHelper.DataHora(m.Data),
                    m.Produto?.Nome ?? "-",
                    m.Tipo == TipoMovimentacao.Entrada ? "Entrada" : "Saída",
                    m.Quantidade,
                    m.Tipo == TipoMovimentacao.Entrada && m.ValorCusto.HasValue ? FormatHelper.Moeda(m.ValorCusto.Value) : "-",
                    m.Tipo == TipoMovimentacao.Entrada
                        ? (string.IsNullOrWhiteSpace(m.FornecedorNome) ? "-" : m.FornecedorNome)
                        : (string.IsNullOrWhiteSpace(m.Motivo) ? "-" : m.Motivo)
                )).ToList();

                _grid.DataSource = _linhas;

                if (_grid.Columns["CustoUnitario"] != null) _grid.Columns["CustoUnitario"].HeaderText = "Custo unitário";
                if (_grid.Columns["Quantidade"] != null) _grid.Columns["Quantidade"].HeaderText = "Qtd.";

                int entradas = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Entrada).Sum(m => m.Quantidade);
                int saidas = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Saida).Sum(m => m.Quantidade);
                AtualizarResumo(entradas, saidas, movimentacoes.Count);
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Carregar relatório de movimentações");
                MessageBox.Show("Não foi possível carregar o relatório.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AtualizarResumo(int entradas, int saidas, int total)
        {
            _lblEntradas.Text = entradas.ToString();
            _lblSaidas.Text = saidas.ToString();
            _lblSaldo.Text = (entradas - saidas).ToString();
            _lblTotal.Text = total.ToString();
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.Value == null) return;
            if (_grid.Columns[e.ColumnIndex].Name != "Tipo") return;

            e.CellStyle!.ForeColor = e.Value.ToString() == "Entrada" ? UiHelper.CorSucesso : UiHelper.CorPerigo;
            e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
        }

        private void BtnExportarCsv_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_linhas.Count == 0)
                {
                    MessageBox.Show("Nenhum registro encontrado para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var dialogo = new SaveFileDialog
                {
                    Filter = "Arquivo CSV (*.csv)|*.csv",
                    Title = "Salvar relatório em CSV",
                    FileName = $"Relatorio_Movimentacoes_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialogo.ShowDialog() != DialogResult.OK) return;

                // Exporta exatamente o que está na tela (com os filtros aplicados)
                string conteudoCsv = RelatorioService.ExportarMovimentacoesParaCsv(_linhas);
                File.WriteAllText(dialogo.FileName, conteudoCsv, Encoding.UTF8);

                MessageBox.Show("Relatório exportado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Exportar relatório CSV");
                MessageBox.Show("Não foi possível exportar o relatório.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
