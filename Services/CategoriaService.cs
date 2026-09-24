using Microsoft.EntityFrameworkCore;
using ControleEstoque.Desktop.Data;
using ControleEstoque.Desktop.Models;

namespace ControleEstoque.Desktop.Services
{
    public class CategoriaService
    {
        public List<Categoria> GetAll(string? filtro = null)
        {
            using var db = new AppDbContext();
            var query = db.Categorias.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = filtro.Trim();
                query = query.Where(c => EF.Functions.Like(c.Nome, $"%{filtro}%"));
            }

            return query.OrderBy(c => c.Nome).ToList();
        }

        public Categoria? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.Categorias.AsNoTracking().FirstOrDefault(c => c.Id == id);
        }

        public void Add(Categoria categoria)
        {
            using var db = new AppDbContext();

            categoria.Nome = categoria.Nome.Trim();

            // Valida se já existe uma categoria com o mesmo nome
            bool existe = db.Categorias.Any(c => c.Nome.ToLower() == categoria.Nome.ToLower());
            if (existe)
            {
                throw new InvalidOperationException($"Já existe uma categoria cadastrada com o nome '{categoria.Nome}'.");
            }

            db.Categorias.Add(categoria);
            db.SaveChanges();
        }

        public void Update(Categoria categoria)
        {
            using var db = new AppDbContext();

            categoria.Nome = categoria.Nome.Trim();

            // Valida se já existe outra categoria com o mesmo nome
            bool existe = db.Categorias.Any(c => c.Nome.ToLower() == categoria.Nome.ToLower() && c.Id != categoria.Id);
            if (existe)
            {
                throw new InvalidOperationException($"Já existe outra categoria com o nome '{categoria.Nome}'.");
            }

            db.Categorias.Update(categoria);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var categoria = db.Categorias.Find(id);

            if (categoria != null)
            {
                db.Categorias.Remove(categoria);
                db.SaveChanges();
            }
        }
    }
}
