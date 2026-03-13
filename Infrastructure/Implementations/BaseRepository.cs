using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    public abstract class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context = context;
        protected abstract DbSet<T> DbSet { get; }

        public T? GetById(int id) => DbSet.Find(id);

        public IEnumerable<T> GetAll() => DbSet.ToList();

        public T Add(T entity)
        {
            DbSet.Add(entity);
            return entity;
        }

        public void Update(T entity) => DbSet.Update(entity);

        public void Delete(T entity) => DbSet.Remove(entity);

        public int SaveChanges() => _context.SaveChanges();
    }
}