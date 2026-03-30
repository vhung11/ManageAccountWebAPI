using ManageAccountWebAPI.Data.DTOs;

namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<AccountDTO> GetAllByUserId(int userId);
        AccountDTO? GetById(int userId, int accountId);
        AccountDTO Create(int userId, CreateAccountRequestDTO request);
        bool Delete(int userId, int accountId);
        IEnumerable<AccountDTO> GetAccountsRankedByBalance();
        IEnumerable<AccountDTO> GetAccountsBelowBalance(decimal threshold);
        IEnumerable<AccountDTO> GetTopNCheckingAccounts(int topN);
        void ApplyInterest();
    }
}
