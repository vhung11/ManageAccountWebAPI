namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface chung cho các Repository với các thao tác CRUD cơ bản
    /// </summary>
    public interface IBaseRepository<T> where T : class
    {
        T? GetById(int id);
        IEnumerable<T> GetAll();
        T Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        int SaveChanges();
    }
}
