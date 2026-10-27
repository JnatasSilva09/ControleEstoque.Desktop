using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ControleEstoque.Desktop.Services;

namespace ControleEstoque.Desktop.Forms
{
    public partial class RelatoriosControl : UserControl
    {
        // Declaração do serviço de movimentações
        private readonly MovimentacaoService _movimentacaoService;

        public RelatoriosControl(Servicos servicos)
        {
            
            // Instancia o serviço para podermos usar nas consultas
            _movimentacaoService = servicos.Movimentacoes;
        }

        private void BtnExportarCsv_Click(object sender, EventArgs e)
        {
            try
            {
                var dados = _movimentacaoService.ObterMovimentacoes(); 

                if (dados == null || !dados.Any())
                {
                    MessageBox.Show("Nenhum registro encontrado para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Arquivo CSV (*.csv)|*.csv";
                    saveFileDialog.Title = "Salvar Relatório em CSV";
                    saveFileDialog.FileName = $"Relatorio_Movimentacoes_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string conteudoCsv = RelatorioService.ExportarMovimentacoesParaCsv(dados);
                        File.WriteAllText(saveFileDialog.FileName, conteudoCsv, Encoding.UTF8);

                        MessageBox.Show("Relatório exportado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar o relatório: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}