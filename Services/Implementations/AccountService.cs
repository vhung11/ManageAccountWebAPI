using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Mappers;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class AccountService(IAccountRepository accountRepository, IAccountBalanceRepository accountBalanceRepository, IInterestTypeRepository interestTypeRepository, ILogger<AccountService> logger) : IAccountService
    {
        public IEnumerable<AccountDTO> GetAll()
        {
            return GetAllAccountDTOs();
        }

        public AccountDTO? GetById(int id)
        {
            var account = accountRepository.GetById(id);
            if (account is null)
            {
                logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId}.", id);
                return null;
            }

            var accountBalances = accountBalanceRepository.GetByAccountId(id);
            logger.LogDebug("Loaded {BalanceCount} account balances for account {AccountId}.", accountBalances.Count(), id);
            return AccountMapper.ToDTO(account, accountBalances);
        }

        public AccountDTO Create(CreateAccountRequestDTO request)
        {
            var accountName = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(accountName))
            {
                logger.LogWarning("Rejected account creation because the supplied account name was empty.");
                throw new ArgumentException("Tên tài khoản là bắt buộc.", nameof(request));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Creating new account with name '{AccountName}' and initial balances: Savings = {SavingsBalance}, Checking = {CheckingBalance}.",
                    accountName,
                    request.SavingsBalance,
                    request.CheckingBalance);
            }

            var savingsInterestType = EnsureInterestType(4.7m);
            var checkingInterestType = EnsureInterestType(5.1m);

            var account = accountRepository.Add(new Account
            {
                Name = accountName
            });

            var balances = new List<AccountBalance>
            {
                new()
                {
                    Account = account,
                    Type = AccountType.Savings,
                    Balance = request.SavingsBalance,
                    InterestType = savingsInterestType
                },
                new()
                {
                    Account = account,
                    Type = AccountType.Checking,
                    Balance = request.CheckingBalance,
                    InterestType = checkingInterestType
                }
            };

            foreach (var balance in balances)
            {
                accountBalanceRepository.Add(balance);
            }

            accountRepository.SaveChanges();

            logger.LogDebug("Created account {AccountId} with {BalanceCount} balances for {AccountName}.",
            account.Id,
            balances.Count,
            account.Name);

            return AccountMapper.ToDTO(account, balances);
        }

        private InterestType EnsureInterestType(decimal rate)
        {
            var interestType = interestTypeRepository.GetByRate(rate);
            if (interestType is not null)
            {
                logger.LogDebug("Reused existing interest type with rate {InterestRate}.", rate);
                return interestType;
            }

            logger.LogInformation("Creating interest type with rate {InterestRate}.", rate);

            return interestTypeRepository.Add(new InterestType
            {
                Rate = rate
            });
        }

        public AccountDTO? Update(int id, UpdateAccountRequestDTO request)
        {
            logger.LogInformation("Updating account with id {AccountId}.", id);

            var account = accountRepository.GetById(id);
            if (account is null)
            {
                logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId} để cập nhật.", id);
                return null;
            }

            var accountName = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(accountName))
            {
                logger.LogWarning("Rejected update for account {AccountId} because the supplied account name was empty.", id);
                throw new ArgumentException("Tên tài khoản là bắt buộc.", nameof(request));
            }

            account.Name = accountName;

            accountRepository.Update(account);
            accountRepository.SaveChanges();

            var updatedBalances = accountBalanceRepository.GetByAccountId(id).ToList();

            logger.LogInformation("Updated account {AccountId} successfully.", id);
            return AccountMapper.ToDTO(account, updatedBalances);
        }

        public bool Delete(int id)
        {
            logger.LogInformation("Deleting account with id {AccountId}.", id);

            var account = accountRepository.GetById(id);
            if (account is null)
            {
                logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId} để xóa.", id);
                return false;
            }

            accountRepository.Delete(account);
            accountRepository.SaveChanges();

            logger.LogInformation("Deleted account with id {AccountId}.", id);
            return true;
        }

        public IEnumerable<AccountDTO> GetAccountsRankedByBalance()
        {
            var rankedAccounts = GetAllAccountDTOs()
                .OrderByDescending(a => a.TotalBalance)
                .ToList();
            logger.LogInformation("Loaded {Count} accounts ranked by balance.", rankedAccounts.Count);

            return rankedAccounts;
        }

        public IEnumerable<AccountDTO> GetAccountsBelowBalance(decimal threshold)
        {
            var belowBalanceAccounts = GetAllAccountDTOs()
                .Where(a => a.TotalBalance < threshold)
                .ToList();

            if (belowBalanceAccounts.Count == 0)
            {
                logger.LogInformation("No accounts found with total balance below {Threshold}.", threshold);
            }
            logger.LogInformation("Loaded {Count} accounts with balances below {Threshold}.", belowBalanceAccounts.Count, threshold);

            return belowBalanceAccounts;
        }

        public IEnumerable<AccountDTO> GetTopNCheckingAccounts(int topN)
        {
            var topCheckingAccounts = GetAllAccountDTOs()
                .OrderBy(a => a.CheckingBalance)
                .Take(topN)
                .ToList();
            logger.LogInformation("Retrieved {Count} checking accounts for top {TopN} lowest balance request.", topCheckingAccounts.Count, topN);

            return topCheckingAccounts;
        }

        private IEnumerable<AccountDTO> GetAllAccountDTOs()
        {
            var accounts = accountRepository.GetAll();
            var accountBalances = accountBalanceRepository.GetAll();
            logger.LogDebug("Loaded {AccountCount} accounts and {BalanceCount} account balances from database", accounts.Count(), accountBalances.Count());

            return AccountMapper.ToDTOList(accounts, accountBalances).ToList();
        }
    }
}
