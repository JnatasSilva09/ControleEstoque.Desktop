using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Dominio.Models;

namespace ControleEstoque.Desktop.Forms
{
    public class MovimentacoesControl : UserControl
    {
        private readonly MovimentacaoService _service;
        private readonly TextBox _txtProduto = new();
        private readonly DateTimePicker _dtInicio = new();
        private readonly DateTimePicker _dtFim = new();
        private readonly CheckBox _chkUsarPeriodo = new();
        private readonly Label _lblAte = new() { Text = "até", AutoSize = true };
        private readonly DataGridView _grid = new();

        public MovimentacoesControl(Servicos servicos)
        {
            _service = servicos.Movimentacoes;
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Movimentações",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            _txtProduto.Location = new Point(25, 60);
            _txtProduto.Size = new Size(220, 28);
            _txtProduto.PlaceholderText = "Filtrar por produto...";
            _txtProduto.TextChanged += (s, e) => Carregar();
            Controls.Add(_txtProduto);

            _chkUsarPeriodo.Text = "Filtrar por período";
            _chkUsarPeriodo.Location = new Point(260, 63);
            _chkUsarPeriodo.AutoSize = true;
            _chkUsarPeriodo.CheckedChanged += ChkUsarPeriodo_CheckedChanged;
            Controls.Add(_chkUsarPeriodo);

            _dtInicio.Location = new Point(410, 60);
            _dtInicio.Size = new Size(120, 26);
            _dtInicio.Format = DateTimePickerFormat.Short;
            _dtInicio.Value = DateTime.Now.AddDays(-30);
            _dtInicio.Enabled = false;
            _dtInicio.ValueChanged += (s, e) => Carregar();
            Controls.Add(_dtInicio);

            _lblAte.Location = new Point(538, 65);
            _lblAte.Enabled = false;
            Controls.Add(_lblAte);

            _dtFim.Location = new Point(565, 60);
            _dtFim.Size = new Size(120, 26);
            _dtFim.Format = DateTimePickerFormat.Short;
            _dtFim.Value = DateTime.Now;
            _dtFim.Enabled = false;
            _dtFim.ValueChanged += (s, e) => Carregar();
            Controls.Add(_dtFim);

            _grid.Location = new Point(25, 100);
            _grid.Size = new Size(1080, 480);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.CellFormatting += Grid_CellFormatting;
            Controls.Add(_grid);

            UiHelper.EstilizarGrid(_grid);

            Load += (s, e) => Carregar();
        }

        private void ChkUsarPeriodo_CheckedChanged(object? sender, EventArgs e)
        {
            bool ativo = _chkUsarPeriodo.Checked;
            _dtInicio.Enabled = ativo;
            _dtFim.Enabled = ativo;
            _lblAte.Enabled = ativo;
            Carregar();
        }

        private void Carregar()
        {
            try
            {
                DateTime? inicio = _chkUsarPeriodo.Checked ? _dtInicio.Value.Date : null;
                DateTime? fim = _chkUsarPeriodo.Checked ? _dtFim.Value.Date.AddDays(1).AddTicks(-1) : null;

                var movimentacoes = _service.GetHistorico(_txtProduto.Text, inicio, fim);

                _grid.DataSource = movimentacoes.Select(m => new
                {
                    Data = FormatHelper.DataHora(m.Data),
                    Produto = m.Produto?.Nome ?? "-",
                    Tipo = m.Tipo == TipoMovimentacao.Entrada ? "Entrada" : "Saída",
                    Quantidade = m.Tipo == TipoMovimentacao.Entrada ? $"+{m.Quantidade}" : $"-{m.Quantidade}",
                    Detalhe = m.Tipo == TipoMovimentacao.Entrada
                        ? (string.IsNullOrWhiteSpace(m.FornecedorNome) ? "-" : m.FornecedorNome)
                        : (string.IsNullOrWhiteSpace(m.Motivo) ? "-" : m.Motivo)
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar movimentações: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Tipo" && e.Value != null)
            {
                string tipo = e.Value.ToString()!;
                if (tipo == "Entrada")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(39, 174, 96);
                    e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
                }
                else if (tipo == "Saída")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(192, 57, 43);
                    e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
                }
            }
        }
    }
}
