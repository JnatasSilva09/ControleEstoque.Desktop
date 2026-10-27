using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public partial class ProdutosControl : UserControl
    {
        private readonly ProdutoService _produtoService;
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtBusca = new();
        private readonly Button _btnNovo = new();
        private readonly Button _btnEditar = new();
        private readonly Button _btnExcluir = new();

        private readonly Servicos _servicos;

        public ProdutosControl(Servicos servicos)
        {
            _servicos = servicos;
            _produtoService = servicos.Produtos;
            // Configurações principais do Controle
            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            // 1. Título da Tela
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
            _txtBusca.PlaceholderText = "Pesquisar por nome, código ou barras...";
            // A cada letra digitada a lista é atualizada com o filtro
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
            _btnNovo.Click += (s, e) => AbrirEdicao(null);

            _btnEditar.Text = "Editar";
            _btnEditar.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _btnEditar.BackColor = Color.FromArgb(59, 130, 246); // Azul
            _btnEditar.ForeColor = Color.White;
            _btnEditar.FlatStyle = FlatStyle.Flat;
            _btnEditar.FlatAppearance.BorderSize = 0;
            _btnEditar.Size = new Size(90, 32);
            _btnEditar.Location = new Point(415, 14);
            _btnEditar.Cursor = Cursors.Hand;
            _btnEditar.Click += (s, e) => EditarSelecionado();

            _btnExcluir.Text = "Excluir";
            _btnExcluir.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _btnExcluir.BackColor = Color.FromArgb(239, 68, 68); // Vermelho
            _btnExcluir.ForeColor = Color.White;
            _btnExcluir.FlatStyle = FlatStyle.Flat;
            _btnExcluir.FlatAppearance.BorderSize = 0;
            _btnExcluir.Size = new Size(90, 32);
            _btnExcluir.Location = new Point(515, 14);
            _btnExcluir.Cursor = Cursors.Hand;
            _btnExcluir.Click += (s, e) => ExcluirSelecionado();

            pnlAcoes.Controls.Add(_txtBusca);
            pnlAcoes.Controls.Add(_btnNovo);
            pnlAcoes.Controls.Add(_btnEditar);
            pnlAcoes.Controls.Add(_btnExcluir);

            // 3. Configuração da Tabela de Dados (DataGridView)
            ConfigurarGrid();
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditarSelecionado(); };

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
            _grid.MultiSelect = false;
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

        private void CarregarProdutos(string filtro = "")
        {
            try
            {
                // Mostra só o que interessa (o nome da categoria, e não o objeto inteiro)
                var produtos = _produtoService.GetAll(filtro)
                    .Select(p => new
                    {
                        p.Id,
                        Código = p.Codigo,
                        p.Nome,
                        Categoria = p.Categoria?.Nome ?? "-",
                        Fornecedor = p.Fornecedor?.Nome ?? "-",
                        Un = p.UnidadeMedida,
                        Custo = FormatHelper.Moeda(p.PrecoCusto),
                        Venda = FormatHelper.Moeda(p.PrecoVenda),
                        Estoque = p.Quantidade,
                        Mínimo = p.EstoqueMinimo,
                        Situação = p.SituacaoEstoque
                    })
                    .ToList();

                _grid.DataSource = null;
                _grid.DataSource = produtos;

                // O Id é só para uso interno
                if (_grid.Columns["Id"] != null)
                {
                    _grid.Columns["Id"]!.Visible = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Carregar produtos");
                MessageBox.Show($"Erro ao carregar os produtos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ObterIdSelecionado()
        {
            if (_grid.CurrentRow?.Cells["Id"].Value is int id)
            {
                return id;
            }
            return null;
        }

        private void AbrirEdicao(int? produtoId)
        {
            using (var form = new ProdutoEditForm(_servicos, produtoId))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    CarregarProdutos(_txtBusca.Text); // Atualiza a tabela com o registro salvo
                }
            }
        }

        private void EditarSelecionado()
        {
            var id = ObterIdSelecionado();
            if (id == null)
            {
                MessageBox.Show("Selecione um produto na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AbrirEdicao(id);
        }

        private void ExcluirSelecionado()
        {
            var id = ObterIdSelecionado();
            if (id == null)
            {
                MessageBox.Show("Selecione um produto na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var nome = _grid.CurrentRow?.Cells["Nome"].Value?.ToString();
            var confirmar = MessageBox.Show(
                $"Deseja excluir o produto \"{nome}\"?\n\nEle deixará de aparecer na lista, mas o histórico de movimentações será mantido.",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                _produtoService.Delete(id.Value);
                CarregarProdutos(_txtBusca.Text);
            }
            catch (Exception ex)
            {
                LogHelper.Erro(ex, "Excluir produto");
                MessageBox.Show($"Não foi possível excluir o produto: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
