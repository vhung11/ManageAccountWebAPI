using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
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
            return PerformTransaction(accountId, amount, AccountTypes.Savings, isDeposit: true);
        }

        public bool DepositToChecking(int accountId, decimal amount)
        {
            return PerformTransaction(accountId, amount, AccountTypes.Checking, isDeposit: true);
        }

        public bool WithdrawFromSavings(int accountId, decimal amount)
        {
            return PerformTransaction(accountId, amount, AccountTypes.Savings, isDeposit: false);
        }

        public bool WithdrawFromChecking(int accountId, decimal amount)
        {
            return PerformTransaction(accountId, amount, AccountTypes.Checking, isDeposit: false);
        }

        public decimal GetTotalSavingsBalance()
        {
            var allBalances = _accountBalanceRepository.GetAll();
            var totalSavingsBalance = allBalances
                .Where(b => b.Type == AccountTypes.Savings)
                .Sum(b => b.Balance);
            
            _logger.LogInformation("Calculated total savings balance across all accounts: {TotalSavingsBalance}.", totalSavingsBalance);
            return totalSavingsBalance;
        }

        private bool PerformTransaction(int accountId, decimal amount, string accountType, bool isDeposit)
        {
            string operationName = isDeposit ? "deposit" : "withdrawal";
            string typeName = accountType == AccountTypes.Savings ? "savings" : "checking";
            string operation = $"{typeName} {operationName}";

            if (amount <= 0)
            {
                _logger.LogWarning("Rejected {Operation} for account {AccountId} because amount {Amount} is not positive.", operation, accountId, amount);
                return false;
            }

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                _logger.LogWarning("Attempted {Operation} for non-existent account with id {AccountId}.", operation, accountId);
                return false;
            }

            var balance = _accountBalanceRepository.GetByAccountIdAndType(accountId, accountType);
            if (balance == null)
            {
                _logger.LogWarning("Attempted {Operation} for account {AccountId} which does not have a {AccountType} balance.", operation, accountId, typeName);
                return false;
            }

            if (!isDeposit && balance.Balance < amount)
            {
                _logger.LogWarning("Attempted to withdraw {Amount} from {AccountType} for account {AccountId} but only {CurrentBalance} is available.",
                    amount, typeName, accountId, balance.Balance);
                return false;
            }

            balance.Balance = isDeposit ? balance.Balance + amount : balance.Balance - amount;
            _accountBalanceRepository.Update(balance);
            _accountBalanceRepository.SaveChanges();

            _logger.LogInformation("{OperationVerb} {Amount} {Direction} {AccountType} for account {AccountId}. New balance: {NewBalance}.",
                isDeposit ? "Deposited" : "Withdrew", amount, isDeposit ? "to" : "from", typeName, accountId, balance.Balance);

            return true;
        }
    }
}
