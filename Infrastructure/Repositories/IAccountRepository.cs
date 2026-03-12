using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho Account Repository
    /// </summary>
    public interface IAccountRepository
    {
        // Basic CRUD Operations
        Account? GetById(int id);
        IEnumerable<Account> GetAll();
        Account Add(Account account);
        void Update(Account account);
        void Delete(Account account);
        bool Exists(int id);
        
        // Save Changes
        int SaveChanges();
    }
}
