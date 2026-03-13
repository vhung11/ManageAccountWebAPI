using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cung cấp các thao tác CRUD chung cho tất cả Repository.
    /// Assumes all entities use an integer primary key (int Id) as required by EF Core's Find method.
    /// </summary>
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected abstract DbSet<T> DbSet { get; }

        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

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
