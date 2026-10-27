using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class EntradaControl : UserControl
    {
        private readonly ProdutoService _produtoService;
        private readonly MovimentacaoService _movimentacaoService;

        private readonly ComboBox _cboProduto = new();
        private readonly Label _lblEstoqueAtual = new();
        private readonly NumericUpDown _numQuantidade = new();
        private readonly NumericUpDown _numValorCusto = new();
        private readonly TextBox _txtFornecedor = new();
        private readonly DateTimePicker _dtData = new();

        public EntradaControl(Servicos servicos)
        {
            _produtoService = servicos.Produtos;
            _movimentacaoService = servicos.Movimentacoes;
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);
            AutoScroll = true;

            var titulo = new Label
            {
                Text = "Entrada de Estoque",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            var painel = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(500, 400),
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
            _cboProduto.SelectedIndexChanged += (s, e) => AtualizarDadosProdutoSelecionado();
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

            var lblValorCusto = new Label { Text = "Valor de Custo (R$)", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblValorCusto);
            y += 20;

            _numValorCusto.Location = new Point(20, y);
            _numValorCusto.Size = new Size(200, 26);
            _numValorCusto.DecimalPlaces = 2;
            _numValorCusto.Maximum = 9999999;
            painel.Controls.Add(_numValorCusto);
            y += 46;

            var lblFornecedor = new Label { Text = "Fornecedor", Location = new Point(20, y), AutoSize = true };
            painel.Controls.Add(lblFornecedor);
            y += 20;

            _txtFornecedor.Location = new Point(20, y);
            _txtFornecedor.Size = new Size(440, 26);
            painel.Controls.Add(_txtFornecedor);
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

            var btnRegistrar = new Button
            {
                Text = "Registrar Entrada",
                Location = new Point(20, y),
                Size = new Size(180, 38),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegistrar.FlatAppearance.BorderSize = 0;
            btnRegistrar.Click += BtnRegistrar_Click;
            painel.Controls.Add(btnRegistrar);

            painel.Height = y + 70;

            Load += (s, e) => CarregarProdutos();
        }

        private void CarregarProdutos()
        {
            try
            {
                var produtos = _produtoService.GetAll();
                _cboProduto.DataSource = produtos;
                AtualizarDadosProdutoSelecionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar a lista de produtos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AtualizarDadosProdutoSelecionado()
{
    if (_cboProduto.SelectedItem is Produto produto)
    {
        _lblEstoqueAtual.Text = $"Estoque atual: {produto.Quantidade} {produto.UnidadeMedida}";
        
        // Sugere o Preço de Custo se o produto já possuir um valor maior que zero
        if (produto.PrecoCusto > 0)
        {
            _numValorCusto.Value = produto.PrecoCusto;
        }
        else
        {
            _numValorCusto.Value = 0;
        }
    }
    else
    {
        _lblEstoqueAtual.Text = string.Empty;
        _numValorCusto.Value = 0;
    }
}

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (_cboProduto.SelectedItem is not Produto produto)
            {
                MessageBox.Show("Selecione um produto para registrar a entrada.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboProduto.Focus();
                return;
            }

            try
            {
                var qtdEntrada = (int)_numQuantidade.Value;
                var valorCusto = _numValorCusto.Value > 0 ? _numValorCusto.Value : (decimal?)null;
                var fornecedor = string.IsNullOrWhiteSpace(_txtFornecedor.Text) ? null : _txtFornecedor.Text.Trim();

                _movimentacaoService.RegistrarEntrada(
                    produto.Id,
                    qtdEntrada,
                    valorCusto,
                    fornecedor,
                    _dtData.Value);

                MessageBox.Show(
                    $"Entrada registrada com sucesso!\nEstoque de \"{produto.Nome}\": {produto.Quantidade} + {qtdEntrada} = {produto.Quantidade + qtdEntrada} unidades.",
                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimparFormulario();
                CarregarProdutos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao registrar entrada: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimparFormulario()
        {
            _numQuantidade.Value = 1;
            _numValorCusto.Value = 0;
            _txtFornecedor.Clear();
            _dtData.Value = DateTime.Now;
        }
    }
}