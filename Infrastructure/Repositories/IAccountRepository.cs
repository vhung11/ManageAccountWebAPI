using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho Account Repository
    /// </summary>
    public interface IAccountRepository : IBaseRepository<Account>
    {
        bool Exists(int id);
    }
}
