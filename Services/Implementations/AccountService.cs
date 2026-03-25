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
        private readonly ILogger<AccountService> logger;

        public AccountService(IAccountRepository accountRepository, IAccountBalanceRepository accountBalanceRepository, IInterestTypeRepository interestTypeRepository, ILogger<AccountService> logger)
        {
            this.accountRepository = accountRepository;
            this.accountBalanceRepository = accountBalanceRepository;
            this.interestTypeRepository = interestTypeRepository;
            this.logger = logger;
        }

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

            var account = new Account
            {
                Name = accountName
            };

            account.AccountBalances.Add(new AccountBalance
            {
                Type = AccountType.Savings,
                Balance = request.SavingsBalance,
                InterestType = savingsInterestType
            });

            account.AccountBalances.Add(new AccountBalance
            {
                Type = AccountType.Checking,
                Balance = request.CheckingBalance,
                InterestType = checkingInterestType
            });

            accountRepository.Add(account);
            accountRepository.SaveChanges();

            logger.LogDebug("Created account {AccountId} with {BalanceCount} balances for {AccountName}.",
                account.Id,
                account.AccountBalances.Count,
                account.Name);

            return AccountMapper.ToDTO(account, account.AccountBalances);
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
            
            var newInterestType = interestTypeRepository.Add(new InterestType
            {
                Rate = rate
            });
            interestTypeRepository.SaveChanges();
            return newInterestType;
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

        private List<AccountDTO> GetAllAccountDTOs()
        {
            var accounts = accountRepository.GetAll();
            var accountBalances = accountBalanceRepository.GetAll();
            logger.LogDebug("Loaded {AccountCount} accounts and {BalanceCount} account balances from database", accounts.Count(), accountBalances.Count());

            return AccountMapper.ToDTOList(accounts, accountBalances);
        }
    }
}

