using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho AccountBalance Repository
    /// </summary>
    public interface IAccountBalanceRepository
    {
        // Basic CRUD Operations
        AccountBalance? GetById(int id);
        IEnumerable<AccountBalance> GetAll();
        AccountBalance Add(AccountBalance accountBalance);
        void Update(AccountBalance accountBalance);
        void Delete(AccountBalance accountBalance);
        
        // Specific Queries
        IEnumerable<AccountBalance> GetByAccountId(int accountId);
        AccountBalance? GetByAccountIdAndType(int accountId, string type);
        
        // Save Changes
        int SaveChanges();
    }
}
