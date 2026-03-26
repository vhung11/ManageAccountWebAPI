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

        public IEnumerable<Account> GetByUserId(int userId)
        {
            return _context.Accounts.Where(a => a.UserId == userId).ToList();
        }

        public bool Exists(int id)
        {
            return _context.Accounts.Any(a => a.Id == id);
        }
    }
}
