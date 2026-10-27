using System;
using System.Drawing;
using System.Windows.Forms;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Desktop.Helpers;

namespace ControleEstoque.Desktop.Forms
{
    public class DashboardControl : UserControl
    {
        private readonly ProdutoService _produtoService;
        private readonly MovimentacaoService _movimentacaoService;
        private readonly FlowLayoutPanel _cardsPanel = new();
        private readonly ListView _listaReposicao = new();

        public DashboardControl(Servicos servicos)
        {
            _produtoService = servicos.Produtos;
            _movimentacaoService = servicos.Movimentacoes;
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            _cardsPanel.Location = new Point(25, 60);
            _cardsPanel.Size = new Size(1100, 130);
            _cardsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _cardsPanel.FlowDirection = FlowDirection.LeftToRight;
            _cardsPanel.WrapContents = true;
            _cardsPanel.AutoScroll = true;
            Controls.Add(_cardsPanel);

            var subtitulo = new Label
            {
                Text = "Produtos que precisam de reposição",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 205)
            };
            Controls.Add(subtitulo);

            _listaReposicao.Location = new Point(25, 235);
            _listaReposicao.Size = new Size(1080, 360);
            _listaReposicao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _listaReposicao.View = View.Details;
            _listaReposicao.FullRowSelect = true;
            _listaReposicao.GridLines = true;
            _listaReposicao.Columns.Add("Produto", 400);
            _listaReposicao.Columns.Add("Estoque Atual", 200);
            _listaReposicao.Columns.Add("Estoque Mínimo", 200);
            _listaReposicao.Columns.Add("Situação", 200);
            Controls.Add(_listaReposicao);

            Load += (s, e) => Atualizar();
        }

        public void Atualizar()
        {
            try
            {
                _cardsPanel.SuspendLayout();
                _listaReposicao.SuspendLayout();

                _cardsPanel.Controls.Clear();

                var totalProdutos = _produtoService.TotalProdutos();
                var estoqueBaixo = _produtoService.GetEstoqueBaixo();
                var semEstoque = _produtoService.GetSemEstoque();
                var totalEntradas = _movimentacaoService.TotalEntradas();
                var totalSaidas = _movimentacaoService.TotalSaidas();
                var valorEstoque = _produtoService.ValorTotalEstoque();

                _cardsPanel.Controls.Add(CriarCard("Total de Produtos", totalProdutos.ToString(), Color.FromArgb(41, 128, 185)));
                _cardsPanel.Controls.Add(CriarCard("Estoque Baixo", estoqueBaixo.Count.ToString(), Color.FromArgb(243, 156, 18)));
                _cardsPanel.Controls.Add(CriarCard("Sem Estoque", semEstoque.Count.ToString(), Color.FromArgb(192, 57, 43)));
                _cardsPanel.Controls.Add(CriarCard("Entradas (un.)", totalEntradas.ToString(), Color.FromArgb(39, 174, 96)));
                _cardsPanel.Controls.Add(CriarCard("Saídas (un.)", totalSaidas.ToString(), Color.FromArgb(142, 68, 173)));
                _cardsPanel.Controls.Add(CriarCard("Valor do Estoque", FormatHelper.Moeda(valorEstoque), Color.FromArgb(52, 73, 94)));

                _listaReposicao.Items.Clear();

                foreach (var produto in semEstoque)
                {
                    var item = new ListViewItem(new[]
                    {
                        produto.Nome,
                        produto.Quantidade.ToString(),
                        produto.EstoqueMinimo.ToString(),
                        "SEM ESTOQUE"
                    })
                    {
                        ForeColor = Color.FromArgb(192, 57, 43),
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        UseItemStyleForSubItems = true
                    };
                    _listaReposicao.Items.Add(item);
                }

                foreach (var produto in estoqueBaixo)
                {
                    var item = new ListViewItem(new[]
                    {
                        produto.Nome,
                        produto.Quantidade.ToString(),
                        produto.EstoqueMinimo.ToString(),
                        "ESTOQUE BAIXO"
                    })
                    {
                        ForeColor = Color.FromArgb(243, 156, 18),
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                        UseItemStyleForSubItems = true
                    };
                    _listaReposicao.Items.Add(item);
                }

                if (_listaReposicao.Items.Count == 0)
                {
                    var item = new ListViewItem(new[] { "Nenhum produto precisa de reposição no momento.", "", "", "" });
                    _listaReposicao.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao carregar os dados do Dashboard: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cardsPanel.ResumeLayout();
                _listaReposicao.ResumeLayout();
            }
        }

        private Panel CriarCard(string titulo, string valor, Color cor)
        {
            var card = new Panel
            {
                Size = new Size(165, 105),
                Margin = new Padding(0, 0, 15, 10),
                BackColor = cor
            };

            var lblValor = new Label
            {
                Text = valor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(12, 12),
                Size = new Size(140, 40)
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(12, 55),
                Size = new Size(140, 35)
            };

            card.Controls.Add(lblValor);
            card.Controls.Add(lblTitulo);

            return card;
        }
    }
}
