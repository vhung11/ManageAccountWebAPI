using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    public class AccountBalanceRepository(ApplicationDbContext context) : BaseRepository<AccountBalance>(context), IAccountBalanceRepository
    {
        protected override DbSet<AccountBalance> DbSet => _context.AccountBalances;

        #region Specific Queries

        /// <summary>
        /// Lấy tất cả AccountBalance của một Account
        /// </summary>
        public IEnumerable<AccountBalance> GetByAccountId(int accountId)
        {
            return _context.AccountBalances
                .Where(ab => ab.AccountId == accountId)
                .ToList();
        }

        /// <summary>
        /// Lấy AccountBalance theo AccountId và Type (Savings/Checking)
        /// </summary>
        public AccountBalance? GetByAccountIdAndType(int accountId, string type)
        {
            return _context.AccountBalances
                .FirstOrDefault(ab => ab.AccountId == accountId && ab.Type == type);
        }

        #endregion
    }
}
