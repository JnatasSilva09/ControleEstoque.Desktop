using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Models;
using BCrypt.Net;

namespace ControleEstoque.Desktop.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context = new();

        public bool RegistrarNovaEmpresa(string nomeEmpresa, string login, string senha)
        {
            if (_context.Usuarios.Any(u => u.Login == login))
            {
                throw new Exception("Este login já está a ser utilizado.");
            }

            var empresa = new Empresa
            {
                NomeEmpresa = nomeEmpresa,
                StatusAssinatura = StatusAssinatura.Degustacao,
                DataVencimento = DateTime.Now.AddDays(14)
            };

            _context.Empresas.Add(empresa);
            _context.SaveChanges();

            // Gera o Hash seguro da palavra-passe com BCrypt
            string senhaHashCriptografada = BCrypt.Net.BCrypt.HashPassword(senha);

            var usuario = new Usuario
            {
                Nome = "Administrador",
                Login = login,
                SenhaHash = senhaHashCriptografada,
                EmpresaId = empresa.Id
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return true;
        }

        public Usuario? Autenticar(string login, string senha)
        {
            // Procura o utilizador pelo login
            var usuario = _context.Usuarios
                .Include(u => u.Empresa)
                .FirstOrDefault(u => u.Login == login);

            if (usuario == null)
            {
                return null;
            }

            // Valida a palavra-passe em texto simples contra o Hash guardado
            bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash);

            if (!senhaValida)
            {
                return null;
            }

            return usuario;
        }
    }
}