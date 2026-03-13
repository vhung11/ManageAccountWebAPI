using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    /// <summary>
    /// Implementation của Account Repository
    /// </summary>
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(ApplicationDbContext context) : base(context)
        {
        }

        protected override DbSet<Account> DbSet => _context.Accounts;

        /// <summary>
        /// Kiểm tra account có tồn tại hay không
        /// </summary>
        public bool Exists(int id)
        {
            return _context.Accounts.Any(a => a.Id == id);
        }
    }
}
