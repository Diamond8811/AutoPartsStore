using System.Data.Entity;
using System.Linq;
using AutoPartsStore.Models;

namespace AutoPartsStore.Infrastructure
{
    public class Repository
    {
        private readonly AutoPartsStoreDBEntities _context;

        public Repository()
        {
            _context = new AutoPartsStoreDBEntities();
        }

        public IQueryable<Products> GetProducts()
        {
            return _context.Products;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
        }

        public void Remove<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public AutoPartsStoreDBEntities Context => _context;

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}