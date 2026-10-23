using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public class UsuariosControl : UserControl
    {
        private readonly UsuarioService _service = new();
        private readonly Usuario _usuarioLogado;

        private readonly TextBox _txtBusca = new();
        private readonly DataGridView _grid = new();
      private readonly System.Windows.Forms.Timer _timerBusca = new() { Interval = 300 };
        private List<Usuario> _usuarios = new();

        public UsuariosControl(Usuario usuarioLogado)
        {
            _usuarioLogado = usuarioLogado;

            Dock = DockStyle.Fill;
            Padding = new Padding(25);
            BackColor = Color.FromArgb(245, 246, 248);

            var titulo = new Label
            {
                Text = "Usuários",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            };
            Controls.Add(titulo);

            // Campo de Busca
            _txtBusca.Location = new Point(25, 60);
            _txtBusca.Size = new Size(280, 28);
            _txtBusca.PlaceholderText = "Pesquisar por nome ou login...";
            _txtBusca.TextChanged += (s, e) =>
            {
                _timerBusca.Stop();
                _timerBusca.Start();
            };
            _timerBusca.Tick += (s, e) =>
            {
                _timerBusca.Stop();
                Carregar();
            };
            Controls.Add(_txtBusca);

            // Botão Novo Usuário
            var btnNovo = new Button
            {
                Text = "+ Novo Usuário",
                Location = new Point(325, 58),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNovo.FlatAppearance.BorderSize = 0;
            btnNovo.Click += (s, e) => AbrirEdicao(null);
            Controls.Add(btnNovo);

            // Botão Editar
            var btnEditar = new Button { Text = "Editar", Location = new Point(485, 58), Size = new Size(100, 32) };
            btnEditar.Click += (s, e) => EditarSelecionado();
            Controls.Add(btnEditar);

            // Botão Excluir
            var btnExcluir = new Button { Text = "Excluir", Location = new Point(595, 58), Size = new Size(100, 32) };
            btnExcluir.Click += (s, e) => ExcluirSelecionado();
            Controls.Add(btnExcluir);

            // Tabela (DataGridView)
            _grid.Location = new Point(25, 105);
            _grid.Size = new Size(1080, 480);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.CellDoubleClick += Grid_CellDoubleClick;
            Controls.Add(_grid);

            UiHelper.EstilizarGrid(_grid);

            Load += (s, e) => Carregar();
        }

        private void Carregar()
        {
            _usuarios = _service.GetAll(_txtBusca.Text);

            _grid.DataSource = _usuarios.Select(u => new
            {
                u.Nome,
                u.Login,
                Perfil = u.Perfil.ToString(),
                Situacao = u.Ativo ? "Ativo" : "Inativo"
            }).ToList();

            AjustarCabecalhosGrid();
        }

        private void AjustarCabecalhosGrid()
{
    if (_grid.Columns["Nome"] is DataGridViewColumn colNome) colNome.HeaderText = "Nome Completo";
    if (_grid.Columns["Login"] is DataGridViewColumn colLogin) colLogin.HeaderText = "Login de Acesso";
    if (_grid.Columns["Perfil"] is DataGridViewColumn colPerfil) colPerfil.HeaderText = "Perfil";
    if (_grid.Columns["Situacao"] is DataGridViewColumn colSituacao) colSituacao.HeaderText = "Situação";

    foreach (DataGridViewColumn coluna in _grid.Columns)
    {
        coluna.SortMode = DataGridViewColumnSortMode.NotSortable;
    }
}
        private Usuario? ObterSelecionado()
        {
            if (_grid.CurrentRow == null) return null;
            var index = _grid.CurrentRow.Index;
            if (index < 0 || index >= _usuarios.Count) return null;
            return _usuarios[index];
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Garante que o clique foi em uma linha de dados e não no cabeçalho
            if (e.RowIndex >= 0)
            {
                EditarSelecionado();
            }
        }

        private void EditarSelecionado()
        {
            var usuario = ObterSelecionado();
            if (usuario == null)
            {
                MessageBox.Show("Selecione um usuário na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AbrirEdicao(usuario);
        }

        private void AbrirEdicao(Usuario? usuario)
        {
            using var form = new UsuarioEditForm(usuario?.Id);
            if (form.ShowDialog() == DialogResult.OK)
            {
                Carregar();
            }
        }

        private void ExcluirSelecionado()
        {
            var usuario = ObterSelecionado();
            if (usuario == null)
            {
                MessageBox.Show("Selecione um usuário na lista.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (usuario.Id == _usuarioLogado.Id)
            {
                MessageBox.Show("Você não pode excluir o próprio usuário enquanto está logado com ele.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmar = MessageBox.Show(
                $"Deseja realmente excluir o usuário \"{usuario.Nome}\"?",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                _service.Delete(usuario.Id);
                Carregar();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Não foi possível excluir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir usuário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}