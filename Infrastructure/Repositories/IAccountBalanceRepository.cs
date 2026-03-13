using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Infrastructure.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho AccountBalance Repository
    /// </summary>
    public interface IAccountBalanceRepository : IBaseRepository<AccountBalance>
    {
        // Specific Queries
        IEnumerable<AccountBalance> GetByAccountId(int accountId);
        AccountBalance? GetByAccountIdAndType(int accountId, string type);
    }
}
