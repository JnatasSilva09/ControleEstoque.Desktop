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
        public List<Usuario> GetAll(string? filtro = null)
        {
            using var db = new AppDbContext();
            var query = db.Usuarios.AsNoTracking().AsQueryable();

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
            return db.Usuarios.AsNoTracking().FirstOrDefault(u => u.Id == id);
        }

        public void Add(string nome, string login, string senha, PerfilUsuario perfil)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("A senha não pode estar em branco.");

            using var db = new AppDbContext();

            var loginLimpo = login.Trim();
            if (db.Usuarios.Any(u => u.Login.ToLower() == loginLimpo.ToLower()))
                throw new InvalidOperationException($"Já existe um usuário com o login \"{loginLimpo}\".");

            var (hash, salt) = PasswordHelper.CriarHash(senha);

            db.Usuarios.Add(new Usuario
            {
                Nome = nome.Trim(),
                Login = loginLimpo,
                SenhaHash = hash,
                SenhaSalt = salt,
                Perfil = perfil,
                Ativo = true,
                DataCadastro = DateTime.Now
            });

            db.SaveChanges();
        }

        public void AtualizarDadosBasicos(int id, string nome, string login, PerfilUsuario perfil, bool ativo)
        {
            using var db = new AppDbContext();
            var usuario = db.Usuarios.Find(id) ?? throw new InvalidOperationException("Usuário não encontrado.");

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
                    var outrosAdminsAtivos = db.Usuarios.Count(u => u.Perfil == PerfilUsuario.Administrador && u.Ativo && u.Id != id);
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

            using var db = new AppDbContext();
            var usuario = db.Usuarios.Find(id) ?? throw new InvalidOperationException("Usuário não encontrado.");

            var (hash, salt) = PasswordHelper.CriarHash(novaSenha);
            usuario.SenhaHash = hash;
            usuario.SenhaSalt = salt;

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();

            var usuario = db.Usuarios.Find(id);
            if (usuario == null) return;

            // Regra de segurança: nunca deixar o sistema sem nenhum administrador ativo
            if (usuario.Perfil == PerfilUsuario.Administrador && usuario.Ativo)
            {
                var outrosAdminsAtivos = db.Usuarios.Count(u => u.Perfil == PerfilUsuario.Administrador && u.Ativo && u.Id != id);
                if (outrosAdminsAtivos == 0)
                    throw new InvalidOperationException("Não é possível excluir o único administrador ativo do sistema.");
            }

            db.Usuarios.Remove(usuario);
            db.SaveChanges();
        }

        // Confere login e senha. Retorna o usuário se estiver correto e ativo; caso contrário, null.
        public Usuario? Autenticar(string login, string senha)
        {
            using var db = new AppDbContext();
            var loginLimpo = login.Trim();
            var usuario = db.Usuarios.FirstOrDefault(u => u.Login.ToLower() == loginLimpo.ToLower());

            if (usuario == null || !usuario.Ativo) return null;
            if (!PasswordHelper.Verificar(senha, usuario.SenhaHash, usuario.SenhaSalt)) return null;

            return usuario;
        }

        // Garante que sempre existe pelo menos um administrador. Roda na inicialização da aplicação.
        public void GarantirUsuarioAdminPadrao()
        {
            using var db = new AppDbContext();
            if (db.Usuarios.Any()) return;

            var (hash, salt) = PasswordHelper.CriarHash("admin123");

            db.Usuarios.Add(new Usuario
            {
                Nome = "Administrador",
                Login = "admin",
                SenhaHash = hash,
                SenhaSalt = salt,
                Perfil = PerfilUsuario.Administrador,
                Ativo = true,
                DataCadastro = DateTime.Now
            });

            db.SaveChanges();
        }
    }
}
