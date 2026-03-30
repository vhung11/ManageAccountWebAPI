using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Mappers;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IAccountBalanceRepository accountBalanceRepository;
        private readonly IInterestTypeRepository interestTypeRepository;
        private readonly IAuthRepository authRepository;
        private readonly ILogger<AccountService> logger;

        public AccountService(IAccountRepository accountRepository, IAccountBalanceRepository accountBalanceRepository, IInterestTypeRepository interestTypeRepository, IAuthRepository authRepository, ILogger<AccountService> logger)
        {
            this.accountRepository = accountRepository;
            this.accountBalanceRepository = accountBalanceRepository;
            this.interestTypeRepository = interestTypeRepository;
            this.authRepository = authRepository;
            this.logger = logger;
        }

        public IEnumerable<AccountDTO> GetAllByUserId(int userId)
        {
            return GetAllAccountDTOsByUserId(userId);
        }

        public AccountDTO? GetById(int userId, int accountId)
        {
            var account = accountRepository.GetById(accountId);
            if (account is null || account.UserId != userId)
            {
                logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId} cho người dùng {UserId}.", accountId, userId);
                return null;
            }

            var accountBalances = accountBalanceRepository.GetByAccountId(accountId);
            logger.LogDebug("Loaded {BalanceCount} account balances for account {AccountId}.", accountBalances.Count(), accountId);
            var user = authRepository.GetUserById(userId);
            var fullName = user?.FullName ?? string.Empty;
            return AccountMapper.ToDTO(account, accountBalances, fullName);
        }

        public AccountDTO Create(int userId, CreateAccountRequestDTO request)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Creating new account for user {UserId} with initial balances: Savings = {SavingsBalance}, Checking = {CheckingBalance}.",
                    userId,
                    request.SavingsBalance,
                    request.CheckingBalance);
            }

            var savingsInterestType = EnsureInterestType(4.7m);
            var checkingInterestType = EnsureInterestType(5.1m);

            var account = new Account


        private InterestType EnsureInterestType(decimal rate)
        {
            var interestType = interestTypeRepository.GetByRate(rate);
            if (interestType is not null)
            {
                logger.LogDebug("Reused existing interest type with rate {InterestRate}.", rate);
                return interestType;
            }

            logger.LogInformation("Creating interest type with rate {InterestRate}.", rate);
            
            var newInterestType = interestTypeRepository.Add(new InterestType
            {
                Rate = rate
            });
            interestTypeRepository.SaveChanges();
            return newInterestType;
        }

        public bool Delete(int userId, int accountId)
        {
            logger.LogInformation("Deleting account with id {AccountId} for user {UserId}.", accountId, userId);

            var account = accountRepository.GetById(accountId);
            if (account is null || account.UserId != userId)
            {
                logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId} cho người dùng {UserId} để xóa.", accountId, userId);
                return false;
            }

            accountRepository.Delete(account);
            accountRepository.SaveChanges();

            logger.LogInformation("Deleted account with id {AccountId}.", accountId);
            return true;
        }

        public IEnumerable<AccountDTO> GetAccountsRankedByBalance()
        {
            var rankedAccounts = GetAllAccountDTOs()
                .OrderByDescending(a => a.TotalBalance)
                .ToList();
            logger.LogInformation("Loaded {Count} accounts ranked by balance system-wide.", rankedAccounts.Count);

            return rankedAccounts;
        }

        public IEnumerable<AccountDTO> GetAccountsBelowBalance(decimal threshold)
        {
            var belowBalanceAccounts = GetAllAccountDTOs()
                .Where(a => a.TotalBalance < threshold)
                .ToList();

            if (belowBalanceAccounts.Count == 0)
            {
                logger.LogInformation("No accounts found with total balance below {Threshold} system-wide.", threshold);
            }
            logger.LogInformation("Loaded {Count} accounts with balances below {Threshold} system-wide.", belowBalanceAccounts.Count, threshold);

            return belowBalanceAccounts;
        }

        public IEnumerable<AccountDTO> GetTopNCheckingAccounts(int topN)
        {
            var topCheckingAccounts = GetAllAccountDTOs()
                .OrderByDescending(a => a.CheckingBalance)
                .Take(topN)
                .ToList();
            logger.LogInformation("Retrieved {Count} checking accounts for top {TopN} lowest balance request system-wide.", topCheckingAccounts.Count, topN);

            return topCheckingAccounts;
        }

        public void ApplyInterest()
        {
            var balances = accountBalanceRepository.GetAll().ToList();
            if (balances.Count == 0)
            {
                logger.LogInformation("Skipped applying interest because there are no account balances.");
                return;
            }

            var interestRates = new Dictionary<int, decimal>();
            var updatedBalanceCount = 0;

            foreach (var accountBalance in balances)
            {
                if (!interestRates.TryGetValue(accountBalance.InterestTypeId, out var rate))
                {
                    var interestType = interestTypeRepository.GetById(accountBalance.InterestTypeId);
                    if (interestType is null)
                    {
                        logger.LogWarning("Skipped interest for balance {BalanceId} because interest type {InterestTypeId} does not exist.", accountBalance.Id, accountBalance.InterestTypeId);
                        continue;
                    }

                    rate = interestType.Rate;
                    interestRates[accountBalance.InterestTypeId] = rate;
                }

                var interestAmount = Math.Round(accountBalance.Balance * (rate / 100m), 2, MidpointRounding.AwayFromZero);
                if (interestAmount == 0)
                {
                    continue;
                }

                accountBalance.Balance += interestAmount;
                accountBalanceRepository.Update(accountBalance);
                updatedBalanceCount++;
            }

            if (updatedBalanceCount > 0)
            {
                accountBalanceRepository.SaveChanges();
            }

            logger.LogInformation("Applied interest to {UpdatedBalanceCount} balances from account service.", updatedBalanceCount);
        }

        private List<AccountDTO> GetAllAccountDTOsByUserId(int userId)
        {
            var accounts = accountRepository.GetByUserId(userId).ToList();
            var accountIds = accounts.Select(a => a.Id).ToList();
            var accountBalances = accountBalanceRepository.GetAll().Where(b => accountIds.Contains(b.AccountId)).ToList();
            logger.LogDebug("Loaded {AccountCount} accounts and {BalanceCount} account balances from database for user {UserId}", accounts.Count, accountBalances.Count, userId);

            var user = authRepository.GetUserById(userId);
            var fullName = user?.FullName ?? string.Empty;
            return AccountMapper.ToDTOList(accounts, accountBalances, fullName);
        }

        private List<AccountDTO> GetAllAccountDTOs()
        {
            var accounts = accountRepository.GetAll().ToList();
            var accountBalances = accountBalanceRepository.GetAll().ToList();
            logger.LogDebug("Loaded {AccountCount} accounts and {BalanceCount} account balances from database system-wide", accounts.Count, accountBalances.Count);

            var result = new List<AccountDTO>();
            var cachedUserNames = new Dictionary<int, string>();

            foreach (var account in accounts)
            {
                if (!cachedUserNames.TryGetValue(account.UserId, out var fullName))
                {
                    var user = authRepository.GetUserById(account.UserId);
                    fullName = user?.FullName ?? string.Empty;
                    cachedUserNames[account.UserId] = fullName;
                }

                var balances = accountBalances.Where(b => b.AccountId == account.Id);
                result.Add(AccountMapper.ToDTO(account, balances, fullName));
            }

            return result;
        }
    }
}

