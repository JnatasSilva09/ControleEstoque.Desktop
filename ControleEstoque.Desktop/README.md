# 📦 Sistema de Controle de Estoque (Desktop)

Uma aplicação desktop completa desenvolvida em C# e .NET para gestão e controlo de inventário, produtos, categorias e fornecedores.

---

## 🚀 Funcionalidades

- 🔐 **Autenticação de Utilizadores:** Ecrã de login seguro com controlo de acessos.
- 📦 **Gestão de Produtos (CRUD):** Registo, edição, listagem e remoção de produtos.
- 🏷️ **Categorias e Fornecedores:** Organização e vínculo de produtos por categoria e fornecedor.
- 📊 **Controlo de Estoque:** Definição de quantidade em estoque e alerta de estoque mínimo.
- 🛠️ **Tratamento de Exceções:** Sistema com captura global de erros para evitar falhas inesperadas.

---

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** [C#](https://learn.microsoft.com/dotnet/csharp/)
- **Plataforma:** [.NET 8.0](https://dotnet.microsoft.com/)
- **Interface Gráfica:** Windows Forms (WinForms)
- **Base de Dados:** SQLite
- **ORM:** Entity Framework Core

---

## 📂 Estrutura do Projeto

```text
ControleEstoque.Desktop/
├── Data/         # Contexto da base de dados e configurações do EF Core
├── Forms/        # Interface gráfica (Login, Gestão de Produtos, etc.)
├── Helpers/      # Classes utilitárias e tratamento global de erros
├── Models/       # Entidades do sistema (Produto, Utilizador, Categoria, etc.)
├── Services/     # Camada de regras de negócio e operações de dados
└── Program.cs    # Ponto de entrada da aplicação
---

## ✒️ Autor

Desenvolvido por **[Jnatas Silva](https://github.com/JnatasSilva09)**.