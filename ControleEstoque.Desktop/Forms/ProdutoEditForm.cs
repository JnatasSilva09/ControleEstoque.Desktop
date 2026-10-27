using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class ProdutoEditForm : Form
    {
        private readonly ProdutoService _produtoService = new();
        private readonly CategoriaService _categoriaService = new();
        private readonly FornecedorService _fornecedorService = new();
        private readonly int? _produtoId;

        private readonly Panel _pnlConteudo = new();
        private readonly Panel _pnlBotoes = new();

        private readonly TextBox _txtCodigo = new();
        private readonly TextBox _txtCodigoBarras = new();
        private readonly TextBox _txtNome = new();
        private readonly TextBox _txtDescricao = new() { Multiline = true };
        private readonly TextBox _txtUnidade = new() { Text = "UN" };
        private readonly NumericUpDown _numPrecoCusto = new();
        private readonly NumericUpDown _numPrecoVenda = new();
        private readonly NumericUpDown _numQuantidade = new();
        private readonly NumericUpDown _numEstoqueMinimo = new();
        private readonly ComboBox _cboCategoria = new();
        private readonly ComboBox _cboFornecedor = new();

        public ProdutoEditForm(int? produtoId)
        {
            _produtoId = produtoId;

            Text = produtoId == null ? "Novo Produto" : "Editar Produto";
            Width = 500;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5f);

            // ---------- Painel de Botões (Rodapé Fixo) ----------
            _pnlBotoes.Dock = DockStyle.Bottom;
            _pnlBotoes.Height = 60;
            _pnlBotoes.BackColor = Color.FromArgb(235, 237, 240);
            Controls.Add(_pnlBotoes);

            var btnSalvar = new Button
            {
                Text = "Salvar",
                Location = new Point(255, 13),
                Size = new Size(100, 34),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSalvar.FlatAppearance.BorderSize = 0;
            btnSalvar.Click += BtnSalvar_Click;
            _pnlBotoes.Controls.Add(btnSalvar);

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(365, 13),
                Size = new Size(100, 34)
            };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _pnlBotoes.Controls.Add(btnCancelar);

            // ---------- Painel de Conteúdo (Com Scroll) ----------
            _pnlConteudo.Dock = DockStyle.Fill;
            _pnlConteudo.AutoScroll = true;
            _pnlConteudo.Padding = new Padding(20);
            Controls.Add(_pnlConteudo);

            var y = 20;
            const int passo = 48;
            const int larguraCampo = 420;

            AdicionarCampo("Código *", _txtCodigo, ref y, passo, larguraCampo);
            AdicionarCampo("Código de Barras", _txtCodigoBarras, ref y, passo, larguraCampo);
            AdicionarCampo("Nome *", _txtNome, ref y, passo, larguraCampo);

            var lblDescricao = new Label { Text = "Descrição", Location = new Point(20, y), AutoSize = true };
            _pnlConteudo.Controls.Add(lblDescricao);
            y += 20;
            _txtDescricao.Location = new Point(20, y);
            _txtDescricao.Size = new Size(larguraCampo, 60);
            _pnlConteudo.Controls.Add(_txtDescricao);
            y += 70;

            AdicionarCampo("Unidade de Medida (ex: UN, CX, KG)", _txtUnidade, ref y, passo, 150);

            _numPrecoCusto.DecimalPlaces = 2;
            _numPrecoCusto.Maximum = 9999999;
            AdicionarCampoNumerico("Preço de Custo (R$)", _numPrecoCusto, ref y, passo, 200);

            _numPrecoVenda.DecimalPlaces = 2;
            _numPrecoVenda.Maximum = 9999999;
            AdicionarCampoNumerico("Preço de Venda (R$)", _numPrecoVenda, ref y, passo, 200);

            _numQuantidade.Maximum = 9999999;
            AdicionarCampoNumerico("Quantidade em Estoque", _numQuantidade, ref y, passo, 200);

            _numEstoqueMinimo.Maximum = 9999999;
            AdicionarCampoNumerico("Estoque Mínimo", _numEstoqueMinimo, ref y, passo, 200);

            var lblCategoria = new Label { Text = "Categoria", Location = new Point(20, y), AutoSize = true };
            _pnlConteudo.Controls.Add(lblCategoria);
            y += 20;
            _cboCategoria.Location = new Point(20, y);
            _cboCategoria.Size = new Size(larguraCampo, 28);
            _cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboCategoria.DisplayMember = "Nome";
            _pnlConteudo.Controls.Add(_cboCategoria);
            y += passo;

            var lblFornecedor = new Label { Text = "Fornecedor", Location = new Point(20, y), AutoSize = true };
            _pnlConteudo.Controls.Add(lblFornecedor);
            y += 20;
            _cboFornecedor.Location = new Point(20, y);
            _cboFornecedor.Size = new Size(larguraCampo, 28);
            _cboFornecedor.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboFornecedor.DisplayMember = "Nome";
            _pnlConteudo.Controls.Add(_cboFornecedor);

            CarregarListasApoio();
            CarregarProdutoExistente();
        }

        private void AdicionarCampo(string rotulo, TextBox campo, ref int y, int passo, int largura)
        {
            var lbl = new Label { Text = rotulo, Location = new Point(20, y), AutoSize = true };
            _pnlConteudo.Controls.Add(lbl);
            y += 20;
            campo.Location = new Point(20, y);
            campo.Size = new Size(largura, 26);
            _pnlConteudo.Controls.Add(campo);
            y += passo;
        }

        private void AdicionarCampoNumerico(string rotulo, NumericUpDown campo, ref int y, int passo, int largura)
        {
            var lbl = new Label { Text = rotulo, Location = new Point(20, y), AutoSize = true };
            _pnlConteudo.Controls.Add(lbl);
            y += 20;
            campo.Location = new Point(20, y);
            campo.Size = new Size(largura, 26);
            _pnlConteudo.Controls.Add(campo);
            y += passo;
        }

        private void CarregarListasApoio()
        {
            try
            {
                var categorias = _categoriaService.GetAll();
                categorias.Insert(0, new Categoria { Id = 0, Nome = "(Nenhuma)" });
                _cboCategoria.DataSource = categorias;
                _cboCategoria.ValueMember = "Id";

                var fornecedores = _fornecedorService.GetAll();
                fornecedores.Insert(0, new Fornecedor { Id = 0, Nome = "(Nenhum)" });
                _cboFornecedor.DataSource = fornecedores;
                _cboFornecedor.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar listas de apoio: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CarregarProdutoExistente()
        {
            if (_produtoId == null) return;

            try
            {
                var produto = _produtoService.GetById(_produtoId.Value);
                if (produto == null) return;

                _txtCodigo.Text = produto.Codigo;
                _txtCodigoBarras.Text = produto.CodigoBarras;
                _txtNome.Text = produto.Nome;
                _txtDescricao.Text = produto.Descricao;
                _txtUnidade.Text = produto.UnidadeMedida;
                _numPrecoCusto.Value = produto.PrecoCusto;
                _numPrecoVenda.Value = produto.PrecoVenda;
                _numQuantidade.Value = produto.Quantidade;
                _numQuantidade.Enabled = false; // estoque só muda por entrada/saída
                _numEstoqueMinimo.Value = produto.EstoqueMinimo;
                _cboCategoria.SelectedValue = produto.CategoriaId ?? 0;
                _cboFornecedor.SelectedValue = produto.FornecedorId ?? 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar os dados do produto: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

private void BtnSalvar_Click(object? sender, EventArgs e)
{
    try
    {
        // 1. Monta o produto com os dados preenchidos no formulário
        var produto = new Produto
        {
            Codigo = _txtCodigo.Text.Trim(),
            CodigoBarras = string.IsNullOrWhiteSpace(_txtCodigoBarras.Text) ? null : _txtCodigoBarras.Text.Trim(),
            Nome = _txtNome.Text.Trim(),
            Descricao = string.IsNullOrWhiteSpace(_txtDescricao.Text) ? null : _txtDescricao.Text.Trim(),
            UnidadeMedida = string.IsNullOrWhiteSpace(_txtUnidade.Text) ? "UN" : _txtUnidade.Text.Trim(),
            PrecoCusto = _numPrecoCusto.Value,
            PrecoVenda = _numPrecoVenda.Value,
            Quantidade = (int)_numQuantidade.Value,
            EstoqueMinimo = (int)_numEstoqueMinimo.Value,
            
            // Trata o ComboBox de Categoria (se selecionado ID 0 ou nulo, vira null)
            CategoriaId = _cboCategoria.SelectedValue is int catId && catId > 0 ? catId : null,
            
            // Trata o ComboBox de Fornecedor (se selecionado ID 0 ou nulo, vira null)
            FornecedorId = _cboFornecedor.SelectedValue is int fornId && fornId > 0 ? fornId : null
        };

        // 2. Salva no banco de dados através do serviço (Add ou Update)
        if (_produtoId.HasValue && _produtoId.Value > 0)
        {
            produto.Id = _produtoId.Value;
            _produtoService.Update(produto);
        }
        else
        {
            _produtoService.Add(produto);
        }

        MessageBox.Show("Registro salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
    catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.DataAnnotations.ValidationException)
    {
        // Regras do sistema (ex.: código repetido): é um aviso, não um erro do programa
        MessageBox.Show(ex.Message, "Não foi possível salvar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    catch (Exception ex)
    {
        LogHelper.Erro(ex, "Salvar produto");
        MessageBox.Show($"Erro ao salvar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
    }
}