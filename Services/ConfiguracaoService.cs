using System.Linq;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    public class ConfiguracaoService
    {
        // Como só existe UMA configuração de empresa, sempre trabalhamos com o primeiro registro.
        // Se ainda não existir nenhum, criamos um vazio automaticamente.
        public Configuracao GetConfiguracao()
        {
            using var db = new AppDbContext();
            var config = db.Configuracoes.FirstOrDefault();

            if (config == null)
            {
                config = new Configuracao { NomeEmpresa = "Minha Empresa" };
                db.Configuracoes.Add(config);
                db.SaveChanges();
            }

            return config;
        }

        public void Salvar(Configuracao configuracao)
        {
            using var db = new AppDbContext();
            
            // Busca o registro existente pelo ID ou pega o primeiro registro que houver
            var existente = db.Configuracoes.Find(configuracao.Id) ?? db.Configuracoes.FirstOrDefault();

            if (existente == null)
            {
                // Se realmente não existir nenhum registro, adiciona como novo
                db.Configuracoes.Add(configuracao);
            }
            else
            {
                // Copia os dados para o objeto rastreado pelo EF
                existente.NomeEmpresa = configuracao.NomeEmpresa;
                existente.Cnpj = configuracao.Cnpj;
                existente.Telefone = configuracao.Telefone;
                existente.WhatsApp = configuracao.WhatsApp;
                existente.Email = configuracao.Email;
                existente.Endereco = configuracao.Endereco;
                existente.Logo = configuracao.Logo;
            }

            db.SaveChanges();
        }
    }
}