# Controle de Estoque — Versão Desktop

Sistema de controle de estoque em C# / .NET 10 / Windows Forms / SQLite / Entity Framework Core.

## Login

O sistema agora exige login. Na primeira vez que você rodar (`dotnet run`), um usuário administrador
padrão é criado automaticamente:

- **Login:** `admin`
- **Senha:** `admin123`

**Troque essa senha assim que possível** (menu "Usuários" → editar o usuário `admin` → preencher o
campo Nova Senha) — ela existe só para você conseguir entrar pela primeira vez.

Apenas usuários com perfil **Administrador** veem o menu "Usuários" e podem cadastrar, editar ou
excluir outros usuários. Usuários com perfil **Operador** têm acesso às demais telas do sistema.

O botão "Sair" (canto superior direito, ao lado do nome do usuário logado) faz logout e volta para a
tela de login, sem fechar o programa.

## Como rodar

Este projeto foi escrito fora de um ambiente Windows, então **não foi compilado nem testado aqui**.
Para rodar:

1. Instale o [.NET 10 SDK](https://dotnet.microsoft.com/download) (LTS), caso ainda não tenha.
2. Abra a pasta `ControleEstoque.Desktop` no VS Code (ou `code .` pelo terminal).
3. No terminal, dentro de `ControleEstoque.Desktop`, rode:
   ```
   dotnet restore
   dotnet build
   ```
4. Se o build for bem-sucedido, rode:
   ```
   dotnet run
   ```

O banco de dados (`estoque.db`) é criado automaticamente na primeira execução, com todas as tabelas
(Produtos, Categorias, Fornecedores, Movimentações, Configurações) — não é necessário rodar `dotnet ef`
manualmente. Se quiser adotar Migrations do EF Core no futuro (por exemplo, para versionar mudanças de
estrutura do banco), é só rodar `dotnet ef migrations add NomeDaMigration` depois de trocar, em
`Program.cs`, a chamada `db.Database.EnsureCreated()` pelo fluxo de Migrations que vimos na Fase 3.

## Se aparecer erro ao compilar

Os erros mais comuns nesta etapa costumam ser:

- **Pacotes não encontrados**: rode `dotnet restore` antes de `dotnet build`.
- **"O SDK do .NET não foi encontrado"**: confirme com `dotnet --version` se o SDK 10 está instalado.
- **Erros de sintaxe/tipo em algum arquivo específico**: copie a mensagem completa do terminal — ela
  sempre indica o arquivo e a linha exata do problema.

## Estrutura do projeto

```
ControleEstoque.Desktop/
├── Models/          → Classes que representam as tabelas do banco (Produto, Categoria, Fornecedor, Movimentacao, Configuracao)
├── Data/             → AppDbContext, a conexão com o banco SQLite
├── Services/         → Regras de negócio (cadastro, entrada/saída de estoque, relatórios)
├── Forms/            → Telas do sistema (MainForm + uma tela para cada funcionalidade)
├── Helpers/          → Funções auxiliares (formatação de moeda e data)
└── Program.cs        → Ponto de entrada do sistema
```

## Funcionalidades incluídas

- Dashboard com cards (produtos, estoque baixo, sem estoque, entradas, saídas, valor do estoque) e lista de reposição
- Cadastro de Produtos (criar, editar, excluir, pesquisar)
- Cadastro de Categorias
- Cadastro de Fornecedores
- Entrada de Estoque (aumenta a quantidade automaticamente)
- Saída de Estoque (bloqueia saída maior que o estoque disponível, com mensagem de erro)
- Histórico de Movimentações com filtro por produto e período
- Relatórios (lista de produtos, estoque atual, sem estoque, estoque baixo, entradas, saídas)
- Configurações da empresa (nome, CNPJ, contato, logo)

## O que ainda não está incluso (próximas fases)

- Exportação de relatórios para PDF/Excel
- Leitor de código de barras
- Login/usuários, versão SaaS, nuvem — conforme planejado para o futuro do projeto
