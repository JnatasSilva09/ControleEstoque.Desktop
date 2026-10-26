using System.Drawing;

namespace ControleEstoque.Desktop.Helpers;

public static class UITheme
{
    // Fontes criadas uma única vez na memória para evitar vazamento de GDI
    public static readonly Font FontePadrao = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font FonteNegrito = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font FonteTitulo = new("Segoe UI", 12f, FontStyle.Bold);

    // Cores padrão centralizadas do sistema
    public static readonly Color CorPrimaria = Color.FromArgb(41, 128, 185);
    public static readonly Color CorFundoCard = Color.FromArgb(245, 247, 250);
    public static readonly Color CorTextoPrincipal = Color.FromArgb(44, 62, 80);
}