using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    public class AuthService
    {
        private readonly IAppDbContextFactory _fabrica;

        public AuthService(IAppDbContextFactory fabrica)
        {
            _fabrica = fabrica;
        }

        public bool RegistrarNovaEmpresa(string nomeEmpresa, string login, string senha)
        {
            using var db = _fabrica.Criar();

            var loginLimpo = login.Trim();
            if (db.Usuarios.Any(u => u.Login.ToLower() == loginLimpo.ToLower()))
            {
                throw new InvalidOperationException("Este login já está sendo utilizado.");
            }

            // Empresa e usuário são criados juntos: se um falhar, nenhum dos dois fica salvo
            using var transacao = db.Database.BeginTransaction();

            var empresa = new Empresa
            {
                NomeEmpresa = nomeEmpresa.Trim(),
                StatusAssinatura = StatusAssinatura.Degustacao,
                DataVencimento = DateTime.Now.AddDays(14)
            };

            db.Empresas.Add(empresa);
            db.SaveChanges();

            // Gera o Hash seguro da senha com BCrypt
            string senhaHashCriptografada = PasswordHelper.HashPassword(senha);

            var usuario = new Usuario
            {
                Nome = "Administrador",
                Login = loginLimpo,
                SenhaHash = senhaHashCriptografada,
                Perfil = PerfilUsuario.Administrador, // quem cria a conta é o administrador da empresa
                EmpresaId = empresa.Id
            };

            db.Usuarios.Add(usuario);
            db.SaveChanges();

            transacao.Commit();

            return true;
        }

        public Usuario? Autenticar(string login, string senha)
        {
            using var db = _fabrica.Criar();

            // Procura o usuário pelo login
            var loginLimpo = login.Trim();
            var usuario = db.Usuarios
                .Include(u => u.Empresa)
                .FirstOrDefault(u => u.Login.ToLower() == loginLimpo.ToLower());

            if (usuario == null || !usuario.Ativo)
            {
                return null;
            }

            // Confere a senha digitada contra o Hash guardado
            if (!PasswordHelper.VerifyPassword(senha, usuario.SenhaHash))
            {
                return null;
            }

            // Se a senha ainda estava no formato antigo, aproveita o login para regravar com BCrypt
            if (PasswordHelper.PrecisaAtualizarHash(usuario.SenhaHash))
            {
                usuario.SenhaHash = PasswordHelper.HashPassword(senha);
                db.SaveChanges();
            }

            return usuario;
        }

        // Usada quando o usuário precisa criar uma senha nova no primeiro acesso
        public void AlterarSenha(int usuarioId, string novaSenha)
        {
            var (senhaValida, mensagem) = PasswordHelper.ValidatePasswordStrength(novaSenha);
            if (!senhaValida)
                throw new InvalidOperationException(mensagem);

            using var db = _fabrica.Criar();
            var usuario = db.Usuarios.Find(usuarioId)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            usuario.SenhaHash = PasswordHelper.HashPassword(novaSenha);
            usuario.PrecisaTrocarSenha = false;

            db.SaveChanges();
        }
    }
}
