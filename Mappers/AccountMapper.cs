using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;

namespace ManageAccountWebAPI.Mappers
{
    /// <summary>
    /// Mapper để chuyển đổi giữa Account Entity và AccountDTO
    /// </summary>
    public static class AccountMapper
    {
        /// <summary>
        /// Map từ Account Entity sang AccountDTO
        /// </summary>
        /// <param name="account">Account entity</param>
        /// <param name="accountBalances">Danh sách AccountBalance của account</param>
        /// <returns>AccountDTO</returns>
        public static AccountDTO ToDTO(Account account, IEnumerable<AccountBalance> accountBalances)
        {
            var balanceList = accountBalances.ToList();
            
            return new AccountDTO
            {
                Id = account.Id,
                Name = account.Name ?? string.Empty,
                SavingsBalance = balanceList.FirstOrDefault(b => b.Type == AccountType.Savings)?.Balance ?? 0,
                CheckingBalance = balanceList.FirstOrDefault(b => b.Type == AccountType.Checking)?.Balance ?? 0
            };
        }

        /// <summary>
        /// Map danh sách Account Entity sang danh sách AccountDTO
        /// </summary>
        /// <param name="accounts">Danh sách accounts</param>
        /// <param name="allAccountBalances">Tất cả AccountBalances trong hệ thống</param>
        /// <returns>Danh sách AccountDTO</returns>
        public static List<AccountDTO> ToDTOList(
            IEnumerable<Account> accounts, 
            IEnumerable<AccountBalance> allAccountBalances)
        {
            var result = new List<AccountDTO>();
            var balanceList = allAccountBalances.ToList();

            foreach (var account in accounts)
            {
                var accountBalances = balanceList.Where(b => b.AccountId == account.Id);
                result.Add(ToDTO(account, accountBalances));
            }

            return result;
        }
    }
}
