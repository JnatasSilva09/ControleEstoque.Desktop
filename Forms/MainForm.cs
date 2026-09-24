using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class MainForm : Form
    {
        private readonly Usuario _usuarioLogado;
        private readonly Panel _sidebar = new();
        private readonly Panel _content = new();
        private readonly Panel _topBar = new();
        private readonly Label _tituloSidebar = new();
        private Button? _botaoAtivo;

        public bool SolicitouLogout { get; private set; }

        public MainForm(Usuario usuarioLogado)
        {
            _usuarioLogado = usuarioLogado;

            Text = "Controle de Estoque";
            Width = 1200;
            Height = 750;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);
            Font = new Font("Segoe UI", 9.5f);

            // ---------- Sidebar (menu lateral) ----------
            _sidebar.Dock = DockStyle.Left;
            _sidebar.Width = 220;
            _sidebar.BackColor = Color.FromArgb(33, 47, 61);
            Controls.Add(_sidebar);

            // Carrega o nome da empresa com fallback para segurança
            string nomeEmpresa = "CONTROLE DE\nESTOQUE";
            try
            {
                var config = new ConfiguracaoService().GetConfiguracao();
                if (!string.IsNullOrWhiteSpace(config?.NomeEmpresa) && config.NomeEmpresa != "Minha Empresa")
                {
                    nomeEmpresa = config.NomeEmpresa.ToUpper();
                }
            }
            catch
            {
                // Se o banco falhar na inicialização, mantém o nome padrão do sistema
            }

            _tituloSidebar.Text = nomeEmpresa;
            _tituloSidebar.ForeColor = Color.White;
            _tituloSidebar.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            _tituloSidebar.Dock = DockStyle.Top;
            _tituloSidebar.Height = 90;
            _tituloSidebar.TextAlign = ContentAlignment.MiddleCenter;
            _sidebar.Controls.Add(_tituloSidebar);

            // ---------- Barra superior ----------
            _topBar.Dock = DockStyle.Top;
            _topBar.Height = 44;
            _topBar.BackColor = Color.White;
            Controls.Add(_topBar);

            var lblUsuario = new Label
            {
                Text = $"Logado como: {_usuarioLogado.Nome}  ({_usuarioLogado.Perfil})",
                AutoSize = true,
                Location = new Point(15, 13),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            _topBar.Controls.Add(lblUsuario);

            var btnSair = new Button
            {
                Text = "Sair",
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat
            };
            btnSair.FlatAppearance.BorderSize = 1;
            btnSair.Click += BtnSair_Click;

            var painelBotaoSair = new Panel { Dock = DockStyle.Right, Width = 120, BackColor = Color.White };
            painelBotaoSair.Controls.Add(btnSair);
            btnSair.Location = new Point(15, 7);
            _topBar.Controls.Add(painelBotaoSair);

            // ---------- Área de conteúdo ----------
            _content.Dock = DockStyle.Fill;
            _content.BackColor = Color.FromArgb(245, 246, 248);
            Controls.Add(_content);
            _content.BringToFront();

            // ---------- Itens de Menu ----------
            CriarBotaoMenu("📊   Dashboard", () => MostrarTela(new DashboardControl()));
            CriarBotaoMenu("📦   Produtos", () => MostrarTela(new ProdutosControl()));
            CriarBotaoMenu("📂   Categorias", () => MostrarTela(new CategoriasControl()));
            CriarBotaoMenu("🚚   Fornecedores", () => MostrarTela(new FornecedoresControl()));
            CriarBotaoMenu("⬆   Entrada de Estoque", () => MostrarTela(new EntradaControl()));
            CriarBotaoMenu("⬇   Saída de Estoque", () => MostrarTela(new SaidaControl()));
            CriarBotaoMenu("📋   Movimentações", () => MostrarTela(new MovimentacoesControl()));
            CriarBotaoMenu("📈   Relatórios", () => MostrarTela(new RelatoriosControl()));
            CriarBotaoMenu("⚙   Configurações", () => MostrarTela(new ConfiguracoesControl()));

            if (_usuarioLogado.Perfil == PerfilUsuario.Administrador)
            {
                CriarBotaoMenu("👤   Usuários", () => MostrarTela(new UsuariosControl(_usuarioLogado)));
            }

            // Seleciona a primeira opção da lista visual (Dashboard)
            var primeiroBotao = _sidebar.Controls.OfType<Button>().LastOrDefault();
            if (primeiroBotao != null)
            {
                AtivarBotao(primeiroBotao);
                MostrarTela(new DashboardControl());
            }
        }

        private void BtnSair_Click(object? sender, EventArgs e)
        {
            var confirmar = MessageBox.Show(
                "Deseja realmente sair e voltar para a tela de login?",
                "Sair", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmar != DialogResult.Yes) return;

            SolicitouLogout = true;
            Close();
        }

        private void CriarBotaoMenu(string texto, Action aoClicar)
        {
            var botao = new Button
            {
                Text = texto,
                Dock = DockStyle.Top,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                ForeColor = Color.Gainsboro,
                BackColor = Color.FromArgb(33, 47, 61),
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 62, 80);

            botao.Click += (s, e) =>
            {
                AtivarBotao(botao);
                aoClicar();
            };

            _sidebar.Controls.Add(botao);
            // Define o índice para garantir ordenação correta de cima para baixo
            _sidebar.Controls.SetChildIndex(botao, 1);
        }

        private void AtivarBotao(Button botao)
        {
            if (_botaoAtivo != null)
            {
                _botaoAtivo.BackColor = Color.FromArgb(33, 47, 61);
                _botaoAtivo.ForeColor = Color.Gainsboro;
            }

            botao.BackColor = Color.FromArgb(41, 128, 185);
            botao.ForeColor = Color.White;
            _botaoAtivo = botao;
        }

        private void MostrarTela(UserControl tela)
        {
            // Libera a memória da tela anterior
            if (_content.Controls.Count > 0)
            {
                var telaAtual = _content.Controls[0];
                _content.Controls.Clear();
                telaAtual.Dispose();
            }

            tela.Dock = DockStyle.Fill;
            _content.Controls.Add(tela);
        }
    }
}