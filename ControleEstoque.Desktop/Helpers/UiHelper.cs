namespace ControleEstoque.Desktop.Helpers
{
    // Centraliza a aparência das tabelas (DataGridView) do sistema, para que todas as
    // telas — Produtos, Categorias, Fornecedores, Movimentações, etc — tenham a mesma cara.
    // Sem isso, cada tela ia precisar repetir as mesmas linhas de configuração visual.
    public static class UiHelper
    {
        public static readonly Color CorPrimaria = Color.FromArgb(41, 128, 185);
        public static readonly Color CorSidebar = Color.FromArgb(33, 47, 61);
        public static readonly Color CorFundo = Color.FromArgb(245, 246, 248);
        public static readonly Color CorSucesso = Color.FromArgb(39, 174, 96);
        public static readonly Color CorAlerta = Color.FromArgb(243, 156, 18);
        public static readonly Color CorPerigo = Color.FromArgb(192, 57, 43);

        public static void EstilizarGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(230, 230, 230);
            grid.RowHeadersVisible = false;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 38;
            grid.RowTemplate.Height = 32;

            grid.ColumnHeadersDefaultCellStyle.BackColor = CorSidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
        }
    }
}
