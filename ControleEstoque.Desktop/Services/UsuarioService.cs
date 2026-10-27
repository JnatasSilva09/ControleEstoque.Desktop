using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Helpers;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    public class UsuarioService
    {
        // Só enxerga os usuários da própria empresa
        private static IQueryable<Usuario> UsuariosDaEmpresa(AppDbContext db)
        {
            var empresaId = Sessao.EmpresaId;
            return db.Usuarios.Where(u => u.EmpresaId == empresaId);
        }

        public List<Usuario> GetAll(string? filtro = null)
        {
            using var db = new AppDbContext();
            var query = UsuariosDaEmpresa(db).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                var f = filtro.Trim();
                query = query.Where(u => EF.Functions.Like(u.Nome, $"%{f}%") || EF.Functions.Like(u.Login, $"%{f}%"));
            }

            return query.OrderBy(u => u.Nome).ToList();
        }

        public Usuario? GetById(int id)
        {
            using var db = new AppDbContext();
            return UsuariosDaEmpresa(db).AsNoTracking().FirstOrDefault(u => u.Id == id);
        }

        public void Add(string nome, string login, string senha, PerfilUsuario perfil)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("A senha não pode estar em branco.");

            var (senhaValida, mensagem) = PasswordHelper.ValidatePasswordStrength(senha);
            if (!senhaValida)
                throw new InvalidOperationException(mensagem);

            using var db = new AppDbContext();

            // O login é único no sistema todo, porque é com ele que a pessoa entra
            var loginLimpo = login.Trim();
            if (db.Usuarios.Any(u => u.Login.ToLower() == loginLimpo.ToLower()))
                throw new InvalidOperationException($"Já existe um usuário com o login \"{loginLimpo}\".");

            string hashSenha = PasswordHelper.HashPassword(senha);

            db.Usuarios.Add(new Usuario
            {
                Nome = nome.Trim(),
                Login = loginLimpo,
                SenhaHash = hashSenha,
                Perfil = perfil,
                Ativo = true,
                DataCadastro = DateTime.Now,
                EmpresaId = Sessao.EmpresaId,
                PrecisaTrocarSenha = true // a senha definida pelo administrador é provisória
            });

            db.SaveChanges();
        }

        public void AtualizarDadosBasicos(int id, string nome, string login, PerfilUsuario perfil, bool ativo)
        {
            using var db = new AppDbContext();
            var usuario = UsuariosDaEmpresa(db).FirstOrDefault(u => u.Id == id)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            var loginLimpo = login.Trim();
            if (db.Usuarios.Any(u => u.Id != id && u.Login.ToLower() == loginLimpo.ToLower()))
                throw new InvalidOperationException($"Já existe um usuário com o login \"{loginLimpo}\".");

            // Regra de segurança: Se o usuário era o único Admin ativo, não pode ser desativado ou rebaixado de perfil
            if (usuario.Perfil == PerfilUsuario.Administrador && usuario.Ativo)
            {
                bool desativando = !ativo;
                bool mudandoPerfil = perfil != PerfilUsuario.Administrador;

                if (desativando || mudandoPerfil)
                {
                    var outrosAdminsAtivos = UsuariosDaEmpresa(db).Count(u => u.Perfil == PerfilUsuario.Administrador && u.Ativo && u.Id != id);
                    if (outrosAdminsAtivos == 0)
                        throw new InvalidOperationException("Não é possível inativar ou alterar o perfil do único administrador ativo do sistema.");
                }
            }

            usuario.Nome = nome.Trim();
            usuario.Login = loginLimpo;
            usuario.Perfil = perfil;
            usuario.Ativo = ativo;

            db.SaveChanges();
        }

        public void RedefinirSenha(int id, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(novaSenha))
                throw new ArgumentException("A nova senha não pode estar em branco.");

            var (senhaValida, mensagem) = PasswordHelper.ValidatePasswordStrength(novaSenha);
            if (!senhaValida)
                throw new InvalidOperationException(mensagem);

            using var db = new AppDbContext();
            var usuario = UsuariosDaEmpresa(db).FirstOrDefault(u => u.Id == id)
                ?? throw new InvalidOperationException("Usuário não encontrado.");

            usuario.SenhaHash = PasswordHelper.HashPassword(novaSenha);

            // Se o administrador redefiniu a senha de outra pessoa, ela precisa trocar no próximo login
            usuario.PrecisaTrocarSenha = id != Sessao.UsuarioId;

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();

            var usuario = UsuariosDaEmpresa(db).FirstOrDefault(u => u.Id == id);
            if (usuario == null) return;

            // Regra de segurança: nunca deixar o sistema sem nenhum administrador ativo
            if (usuario.Perfil == PerfilUsuario.Administrador && usuario.Ativo)
            {
                var outrosAdminsAtivos = UsuariosDaEmpresa(db).Count(u => u.Perfil == PerfilUsuario.Administrador && u.Ativo && u.Id != id);
                if (outrosAdminsAtivos == 0)
                    throw new InvalidOperationException("Não é possível excluir o único administrador ativo do sistema.");
            }

            db.Usuarios.Remove(usuario);
            db.SaveChanges();
        }
    }
}
