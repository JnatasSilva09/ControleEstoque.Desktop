using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class SaidaControl : UserControl
    {
        private readonly ProdutoService _produtoService;
        private readonly MovimentacaoService _movimentacaoService;

        private readonly ComboBox _cboProduto = new();
        private readonly Label _lblEstoqueAtual = new();
        private readonly NumericUpDown _numQuantidade = new();
        private readonly ComboBox _cboMotivo = new();
        private readonly DateTimePicker _dtData = new();
        private readonly Button _btnRegistrar = new();

        public SaidaControl(Servicos servicos)
        {
            _produtoService = servicos.Produtos;
            _movimentacaoService = servicos.Movimentacoes;
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Saída de Estoque",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            var painel = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(500, 340),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(painel);

            var y = 20;

            var lblProduto = new Label { Text = "Produto *", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblProduto);
            y += 20;

            _cboProduto.Location = new Point(20, y);
            _cboProduto.Size = new Size(440, 28);
            _cboProduto.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboProduto.DisplayMember = "Nome";
            _cboProduto.ValueMember = "Id";
            _cboProduto.SelectedIndexChanged += (s, e) => AtualizarEstoqueAtual();
            painel.Controls.Add(_cboProduto);
            y += 34;

            _lblEstoqueAtual.Location = new Point(20, y);
            _lblEstoqueAtual.AutoSize = true;
            _lblEstoqueAtual.Font = new Font("Segoe UI", 9.5f, FontStyle.Italic);
            _lblEstoqueAtual.ForeColor = Color.DimGray;
            painel.Controls.Add(_lblEstoqueAtual);
            y += 34;

            var lblQuantidade = new Label { Text = "Quantidade *", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblQuantidade);
            y += 20;

            _numQuantidade.Location = new Point(20, y);
            _numQuantidade.Size = new Size(200, 26);
            _numQuantidade.Minimum = 1;
            _numQuantidade.Maximum = 999999;
            painel.Controls.Add(_numQuantidade);
            y += 46;

            var lblMotivo = new Label { Text = "Motivo", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblMotivo);
            y += 20;

            _cboMotivo.Location = new Point(20, y);
            _cboMotivo.Size = new Size(300, 26);
            _cboMotivo.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboMotivo.Items.AddRange(new object[] { "Venda", "Perda", "Uso Interno", "Devolução ao Fornecedor", "Outro" });
            _cboMotivo.SelectedIndex = 0;
            painel.Controls.Add(_cboMotivo);
            y += 46;

            var lblData = new Label { Text = "Data", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblData);
            y += 20;

            _dtData.Location = new Point(20, y);
            _dtData.Size = new Size(200, 26);
            _dtData.Format = DateTimePickerFormat.Short;
            _dtData.Value = DateTime.Now;
            painel.Controls.Add(_dtData);
            y += 50;

            _btnRegistrar.Text = "Registrar Saída";
            _btnRegistrar.Location = new Point(20, y);
            _btnRegistrar.Size = new Size(180, 38);
            _btnRegistrar.BackColor = Color.FromArgb(192, 57, 43);
            _btnRegistrar.ForeColor = Color.White;
            _btnRegistrar.FlatStyle = FlatStyle.Flat;
            _btnRegistrar.FlatAppearance.BorderSize = 0;
            _btnRegistrar.Click += BtnRegistrar_Click;
            painel.Controls.Add(_btnRegistrar);

            painel.Height = y + 70;

            Load += (s, e) => CarregarProdutos();
        }

        public void CarregarProdutos()
        {
            _cboProduto.DataSource = _produtoService.GetAll();
            AtualizarEstoqueAtual();
        }

        private void AtualizarEstoqueAtual()
        {
            if (_cboProduto.SelectedItem is Produto produto)
            {
                _lblEstoqueAtual.Text = $"Estoque atual: {produto.Quantidade} {produto.UnidadeMedida}";

                if (produto.Quantidade <= 0)
                {
                    _lblEstoqueAtual.ForeColor = Color.Red;
                    _numQuantidade.Minimum = 0;
                    _numQuantidade.Maximum = 0;
                    _numQuantidade.Value = 0;
                    _numQuantidade.Enabled = false;
                    _btnRegistrar.Enabled = false;
                }
                else
                {
                    _lblEstoqueAtual.ForeColor = Color.DimGray;
                    _numQuantidade.Enabled = true;
                    _btnRegistrar.Enabled = true;
                    _numQuantidade.Minimum = 1;
                    _numQuantidade.Maximum = produto.Quantidade;
                    _numQuantidade.Value = 1;
                }
            }
            else
            {
                _lblEstoqueAtual.Text = string.Empty;
                _numQuantidade.Enabled = false;
                _btnRegistrar.Enabled = false;
            }
        }

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (_cboProduto.SelectedItem is not Produto produto)
            {
                MessageBox.Show("Selecione um produto.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var quantidadeAnterior = produto.Quantidade;
                var quantidadeSaida = (int)_numQuantidade.Value;

                _movimentacaoService.RegistrarSaida(
                    produto.Id,
                    quantidadeSaida,
                    _cboMotivo.SelectedItem?.ToString(),
                    _dtData.Value);

                MessageBox.Show(
                    $"Saída registrada com sucesso!\nEstoque de \"{produto.Nome}\": {quantidadeAnterior} - {quantidadeSaida} = {quantidadeAnterior - quantidadeSaida} unidades.",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CarregarProdutos();
            }
            catch (EstoqueInsuficienteException ex)
            {
                MessageBox.Show(ex.Message, "Estoque insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao registrar saída", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}