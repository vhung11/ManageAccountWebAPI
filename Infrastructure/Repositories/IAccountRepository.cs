using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho Account Repository
    /// </summary>
    public interface IAccountRepository : IBaseRepository<Account>
    {
        IEnumerable<Account> GetByUserId(int userId);
        bool Exists(int id);
    }
}
