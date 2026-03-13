using ManageAccountWebAPI.Data.DTOs;
using ManageAccountWebAPI.Data.Entities;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Mappers;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private const string SavingsAccountType = "Tài khoản tiết kiệm";
        private const string CheckingAccountType = "Tài khoản thanh toán";

        private readonly IAccountRepository _accountRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IInterestTypeRepository _interestTypeRepository;

        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            IAccountBalanceRepository accountBalanceRepository,
            IInterestTypeRepository interestTypeRepository,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _interestTypeRepository = interestTypeRepository;
            _logger = logger;
        }

        public IEnumerable<AccountDTO> GetAll()
        {
            var accounts = _accountRepository.GetAll();
            var accountBalances = _accountBalanceRepository.GetAll();
            _logger.LogDebug("Loaded {AccountCount} accounts and {BalanceCount} account balances from database.", accounts.Count(), accountBalances.Count());

            return AccountMapper.ToDTOList(accounts, accountBalances);
        }

        public AccountDTO? GetById(int id)
        {
            var account = _accountRepository.GetById(id);
            if (account is null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId}.", id);
                return null;
            }

            var accountBalances = _accountBalanceRepository.GetByAccountId(id);
            _logger.LogDebug("Loaded {BalanceCount} account balances for account {AccountId}.", accountBalances.Count(), id);
            return AccountMapper.ToDTO(account, accountBalances);
        }

        public AccountDTO Create(CreateAccountRequestDTO request)
        {
            var accountName = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(accountName))
            {
                _logger.LogWarning("Rejected account creation because the supplied account name was empty.");
                throw new ArgumentException("Tên tài khoản là bắt buộc.", nameof(request));
            }

	        _logger.LogInformation("Creating new account with name '{AccountName}' and initial balances: Savings = {SavingsBalance}, Checking = {CheckingBalance}.", 
	        accountName, 
	        request.SavingsBalance, 
	        request.CheckingBalance);

            var savingsInterestType = EnsureInterestType(4.7m);
            var checkingInterestType = EnsureInterestType(5.1m);

            var account = _accountRepository.Add(new Account
            {
                Name = accountName
            });

            var balances = new List<AccountBalance>
            {
                new()
                {
                    Account = account,
                    Type = SavingsAccountType,
                    Balance = request.SavingsBalance,
                    InterestType = savingsInterestType
                },
                new()
                {
                    Account = account,
                    Type = CheckingAccountType,
                    Balance = request.CheckingBalance,
                    InterestType = checkingInterestType
                }
            };

            foreach (var balance in balances)
            {
                _accountBalanceRepository.Add(balance);
            }

            _accountRepository.SaveChanges();

            _logger.LogDebug("Created account {AccountId} with {BalanceCount} balances for {AccountName}.", 
            account.Id, 
            balances.Count, 
            account.Name);

            return AccountMapper.ToDTO(account, balances);
        }

        private InterestType EnsureInterestType(decimal rate)
        {
            var interestType = _interestTypeRepository.GetByRate(rate);
            if (interestType is not null)
            {
                _logger.LogDebug("Reused existing interest type with rate {InterestRate}.", rate);
                return interestType;
            }

            _logger.LogInformation("Creating interest type with rate {InterestRate}.", rate);

            return _interestTypeRepository.Add(new InterestType
            {
                Rate = rate
            });
        }

        public AccountDTO? Update(int id, UpdateAccountRequestDTO request)
        {
            _logger.LogInformation("Updating account with id {AccountId}.", id);

            var account = _accountRepository.GetById(id);
            if (account is null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId} để cập nhật.", id);
                return null;
            }

            var accountName = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(accountName))
            {
                _logger.LogWarning("Rejected update for account {AccountId} because the supplied account name was empty.", id);
                throw new ArgumentException("Tên tài khoản là bắt buộc.", nameof(request));
            }

            account.Name = accountName;

            _accountRepository.Update(account);
            _accountRepository.SaveChanges();

            var updatedBalances = _accountBalanceRepository.GetByAccountId(id).ToList();

            _logger.LogInformation("Updated account {AccountId} successfully.", id);
            return AccountMapper.ToDTO(account, updatedBalances);
        }

        public bool Delete(int id)
        {
            _logger.LogInformation("Deleting account with id {AccountId}.", id);

            var account = _accountRepository.GetById(id);
            if (account is null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản có id = {AccountId} để xóa.", id);
                return false;
            }

            _accountRepository.Delete(account);
            _accountRepository.SaveChanges();

            _logger.LogInformation("Deleted account with id {AccountId}.", id);
            return true;
        }

        public IEnumerable<AccountDTO> GetAccountsRankedByBalance()
        {
            var accounts = _accountRepository.GetAll();
            var accountBalances = _accountBalanceRepository.GetAll();

            var rankedAccounts = AccountMapper.ToDTOList(accounts, accountBalances)
                .OrderByDescending(a => a.TotalBalance)
                .ToList();
            _logger.LogInformation("Loaded {Count} accounts ranked by balance.", rankedAccounts.Count());

            return rankedAccounts;
        }

        public IEnumerable<AccountDTO> GetAccountsBelowBalance(decimal threshold)
        {
            var accounts = _accountRepository.GetAll();
            var accountBalances = _accountBalanceRepository.GetAll();

            var belowBalanceAccounts = AccountMapper.ToDTOList(accounts, accountBalances)
                .Where(a => a.TotalBalance < threshold)
                .ToList();
            if (belowBalanceAccounts.Count == 0)
            {
                _logger.LogInformation("No accounts found with total balance below {Threshold}.", threshold);
            }
            _logger.LogInformation("Loaded {Count} accounts with balances below {Threshold}.", belowBalanceAccounts.Count, threshold);

            return belowBalanceAccounts;
        }

        public IEnumerable<AccountDTO> GetTopNCheckingAccounts(int topN)
        {
            var accounts = _accountRepository.GetAll();
            var accountBalances = _accountBalanceRepository.GetAll();

            var topCheckingAccounts = AccountMapper.ToDTOList(accounts, accountBalances)
                .OrderByDescending(a => a.CheckingBalance)
                .Take(topN)
                .ToList();
            _logger.LogInformation("Retrieved {Count} checking accounts for top {TopN} lowest balance request.", topCheckingAccounts.Count, topN);

            return topCheckingAccounts;
        }
    }
}
