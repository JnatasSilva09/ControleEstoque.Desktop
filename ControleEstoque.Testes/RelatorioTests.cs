using Xunit;
using ControleEstoque.Aplicacao.Services;
using ControleEstoque.Dominio.Models;

namespace ControleEstoque.Testes;

public class RelatorioTests
{
    [Fact]
    public void Csv_tem_cabecalho_e_uma_linha_por_registro()
    {
        var dados = new[]
        {
            new { Produto = "Parafuso", Quantidade = 5 },
            new { Produto = "Porca", Quantidade = 2 }
        };

        var linhas = RelatorioService.ExportarMovimentacoesParaCsv(dados)
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("Produto;Quantidade", linhas[0]);
        Assert.Equal(3, linhas.Length);
        Assert.Equal("\"Parafuso\";\"5\"", linhas[1]);
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+cmd")]
    [InlineData("-2")]
    [InlineData("@SOMA(A1)")]
    public void Texto_que_comeca_como_formula_e_neutralizado(string texto)
    {
        var csv = RelatorioService.ExportarMovimentacoesParaCsv(new[] { new { Nome = texto } });

        Assert.Contains($"\"'{texto}\"", csv);
    }

    [Fact]
    public void Aspas_no_texto_sao_duplicadas()
    {
        var csv = RelatorioService.ExportarMovimentacoesParaCsv(new[] { new { Nome = "Cabo \"HDMI\"" } });

        Assert.Contains("\"Cabo \"\"HDMI\"\"\"", csv);
    }
}
