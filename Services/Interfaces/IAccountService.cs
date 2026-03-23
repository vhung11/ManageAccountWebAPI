using ManageAccountWebAPI.Data.DTOs;

namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<AccountDTO> GetAll();
        AccountDTO? GetById(int id);
        AccountDTO Create(CreateAccountRequestDTO request);
        AccountDTO? Update(int id, UpdateAccountRequestDTO request);
        bool Delete(int id);
        IEnumerable<AccountDTO> GetAccountsRankedByBalance();
        IEnumerable<AccountDTO> GetAccountsBelowBalance(decimal threshold);
        IEnumerable<AccountDTO> GetTopNCheckingAccounts(int topN);
        void ApplyInterest();
    }
}
