using ControleEstoque.Dominio.Contexto;
using ControleEstoque.Dominio.Models;
using ControleEstoque.Infra.Backups;
using ControleEstoque.Infra.Data;


namespace ControleEstoque.Aplicacao.Services
{
    // Reúne os serviços já montados. É criado uma vez no Program.cs e entregue às telas,
    // assim nenhuma tela precisa saber como o banco ou a empresa logada são resolvidos.
    public class Servicos
    {
        public Servicos(IAppDbContextFactory fabrica, IContextoDoTenant tenant)
        {
            Produtos = new ProdutoService(fabrica);
            Movimentacoes = new MovimentacaoService(fabrica);
            Categorias = new CategoriaService(fabrica);
            Fornecedores = new FornecedorService(fabrica);
            Usuarios = new UsuarioService(fabrica, tenant);
            Configuracao = new ConfiguracaoService(fabrica);
            Autenticacao = new AuthService(fabrica);
            Backup = new BackupService(fabrica);
        }

        public ProdutoService Produtos { get; }
        public MovimentacaoService Movimentacoes { get; }
        public CategoriaService Categorias { get; }
        public FornecedorService Fornecedores { get; }
        public UsuarioService Usuarios { get; }
        public ConfiguracaoService Configuracao { get; }
        public AuthService Autenticacao { get; }
        public BackupService Backup { get; }
    }
}
