using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Repositories;

namespace ManageAccountWebAPI.Infrastructure.Implementations
{
    /// <summary>
    /// Implementation của Account Repository
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Lấy account theo Id (không include related data)
        /// </summary>
        public Account? GetById(int id)
        {
            return _context.Accounts.FirstOrDefault(a => a.Id == id);
        }

        /// <summary>
        /// Lấy tất cả accounts (không include related data)
        /// </summary>
        public IEnumerable<Account> GetAll()
        {
            return _context.Accounts.ToList();
        }

        /// <summary>
        /// Thêm account mới vào database
        /// </summary>
        public Account Add(Account account)
        {
            _context.Accounts.Add(account);
            return account;
        }

        /// <summary>
        /// Cập nhật thông tin account
        /// </summary>
        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }

        /// <summary>
        /// Xóa account khỏi database
        /// </summary>
        public void Delete(Account account)
        {
            _context.Accounts.Remove(account);
        }

        /// <summary>
        /// Kiểm tra account có tồn tại hay không
        /// </summary>
        public bool Exists(int id)
        {
            return _context.Accounts.Any(a => a.Id == id);
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
