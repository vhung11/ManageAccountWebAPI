using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    public class AccountBalanceRepository : IAccountBalanceRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountBalanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy AccountBalance theo Id
        /// </summary>
        public AccountBalance? GetById(int id)
        {
            return _context.AccountBalances.FirstOrDefault(ab => ab.Id == id);
        }

        /// <summary>
        /// Lấy tất cả AccountBalances
        /// </summary>
        public IEnumerable<AccountBalance> GetAll()
        {
            return _context.AccountBalances.ToList();
        }

        /// <summary>
        /// Thêm AccountBalance mới
        /// </summary>
        public AccountBalance Add(AccountBalance accountBalance)
        {
            _context.AccountBalances.Add(accountBalance);
            return accountBalance;
        }

        /// <summary>
        /// Cập nhật AccountBalance
        /// </summary>
        public void Update(AccountBalance accountBalance)
        {
            _context.AccountBalances.Update(accountBalance);
        }

        /// <summary>
        /// Xóa AccountBalance
        /// </summary>
        public void Delete(AccountBalance accountBalance)
        {
            _context.AccountBalances.Remove(accountBalance);
        }

        #endregion

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

        #region Save Changes

        /// <summary>
        /// Lưu tất cả thay đổi vào database
        /// </summary>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        #endregion
    }
}
