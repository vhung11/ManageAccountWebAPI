using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private const string SavingsAccountType = "Tài khoản tiết kiệm";
        private const string CheckingAccountType = "Tài khoản thanh toán";

        private readonly IAccountRepository _accountRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IAccountRepository accountRepository, IAccountBalanceRepository accountBalanceRepository, ILogger<TransactionService> logger)
        {
            _accountRepository = accountRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _logger = logger;
        }

        public bool DepositToSavings(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Rejected savings deposit for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to deposit to savings for non-existent account with id {AccountId}.", accountId);
                return false;
            }

            var savingsBalance = _accountBalanceRepository.GetByAccountIdAndType(accountId, SavingsAccountType);
            if (savingsBalance == null)
            {
                _logger.LogWarning("Attempted to deposit to savings for account {AccountId} which does not have a savings balance.", accountId);
                return false;
            }

            savingsBalance.Balance += amount;
            _accountBalanceRepository.Update(savingsBalance);
            _accountBalanceRepository.SaveChanges();

            _logger.LogInformation("Deposited {Amount} to savings for account {AccountId}. New savings balance: {NewBalance}.", 
                amount, 
                accountId, 
                savingsBalance.Balance);

            return true;
        }

        public bool DepositToChecking(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Rejected checking deposit for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to deposit to checking for non-existent account with id {AccountId}.", accountId);
                return false;
            }

            var checkingBalance = _accountBalanceRepository.GetByAccountIdAndType(accountId, CheckingAccountType);
            if (checkingBalance == null)
            {
                _logger.LogWarning("Attempted to deposit to checking for account {AccountId} which does not have a checking balance.", accountId);
                return false;
            }

            checkingBalance.Balance += amount;
            _accountBalanceRepository.Update(checkingBalance);
            _accountBalanceRepository.SaveChanges();

            _logger.LogInformation("Deposited {Amount} to checking for account {AccountId}. New checking balance: {NewBalance}.", 
                amount, 
                accountId, 
                checkingBalance.Balance);

            return true;
        }

        public bool WithdrawFromSavings(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Rejected savings withdrawal for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to withdraw from savings for non-existent account with id {AccountId}.", accountId);
                return false;
            }

            var savingsBalance = _accountBalanceRepository.GetByAccountIdAndType(accountId, SavingsAccountType);
            if (savingsBalance == null)
            {
                _logger.LogWarning("Attempted to withdraw from savings for account {AccountId} which does not have a savings balance.", accountId);
                return false;
            }

            if (savingsBalance.Balance < amount)
            {
                _logger.LogWarning("Attempted to withdraw {Amount} from savings for account {AccountId} but only {CurrentBalance} is available.", 
                    amount, 
                    accountId, 
                    savingsBalance.Balance);
                return false;
            }

            savingsBalance.Balance -= amount;
            _accountBalanceRepository.Update(savingsBalance);
            _accountBalanceRepository.SaveChanges();

            _logger.LogInformation("Withdrew {Amount} from savings for account {AccountId}. New savings balance: {NewBalance}.", 
                amount, 
                accountId, 
                savingsBalance.Balance);

            return true;
        }

        public bool WithdrawFromChecking(int accountId, decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Rejected checking withdrawal for account {AccountId} because amount {Amount} is not positive.", accountId, amount);
                return false;
            }

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted to withdraw from checking for non-existent account with id {AccountId}.", accountId);
                return false;
            }

            var checkingBalance = _accountBalanceRepository.GetByAccountIdAndType(accountId, CheckingAccountType);
            if (checkingBalance == null)
            {
                _logger.LogWarning("Attempted to withdraw from checking for account {AccountId} which does not have a checking balance.", accountId);
                return false;
            }

            if (checkingBalance.Balance < amount)
            {
                _logger.LogWarning("Attempted to withdraw {Amount} from checking for account {AccountId} but only {CurrentBalance} is available.", 
                    amount, 
                    accountId, 
                    checkingBalance.Balance);
                return false;
            }

            checkingBalance.Balance -= amount;
            _accountBalanceRepository.Update(checkingBalance);
            _accountBalanceRepository.SaveChanges();

            _logger.LogInformation("Withdrew {Amount} from checking for account {AccountId}. New checking balance: {NewBalance}.", 
                amount, 
                accountId, 
                checkingBalance.Balance);

            return true;
        }

        public decimal GetTotalSavingsBalance()
        {
            var accounts = _accountRepository.GetAll();
            var savingsBalances = accounts.Select (a => _accountBalanceRepository.GetByAccountIdAndType(a.Id, SavingsAccountType))
                                          .Where(b => b != null)
                                          .Select(b => b!.Balance);
            var totalSavingsBalance = savingsBalances.Sum();
            
            _logger.LogInformation("Calculated total savings balance across all accounts: {TotalSavingsBalance}.", totalSavingsBalance);
            return totalSavingsBalance;
        }
    }
}