using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class TransactionService(IAccountRepository accountRepository, IAccountBalanceRepository accountBalanceRepository, ILogger<TransactionService> logger) : ITransactionService
    {
        public bool DepositToSavings(int accountId, decimal amount)
        {
            return PerFormTransaction(accountId, amount, AccountType.Savings, true);
        }

        public bool DepositToChecking(int accountId, decimal amount)
        {
            return PerFormTransaction(accountId, amount, AccountType.Checking, true);
        }

        public bool WithdrawFromSavings(int accountId, decimal amount)
        {
            return PerFormTransaction(accountId, amount, AccountType.Savings, false);
        }

        public bool WithdrawFromChecking(int accountId, decimal amount)
        {
            return PerFormTransaction(accountId, amount, AccountType.Checking, false);
        }

        public decimal GetTotalSavingsBalance()
        {
            var accounts = accountRepository.GetAll();
            var savingsBalances = accounts.Select(a => accountBalanceRepository.GetByAccountIdAndType(a.Id, AccountType.Savings))
                                          .Where(b => b != null)
                                          .Select(b => b!.Balance);
            var totalSavingsBalance = savingsBalances.Sum();

            logger.LogInformation("Calculated total savings balance across all accounts: {TotalSavingsBalance}.", totalSavingsBalance);
            return totalSavingsBalance;
        }

        private bool PerFormTransaction(int accountId, decimal amount, string accountType, bool isDeposit)
        {
            string operationName = isDeposit ? "Deposit" : "Withdraw";
            string typeName = accountType == AccountType.Savings ? "Savings" : "Checking";
            string operation = $"{operationName} {typeName}";

            if (amount <= 0)
            {
                logger.LogWarning("Rejected {Operation} for account {AccountId} because amount {Amount} is not positive.", operation, accountId, amount);
                return false;
            }

            var account = accountRepository.GetById(accountId);
            if (account == null)
            {
                logger.LogWarning("Attempted to {Operation} for non-existent account with id {AccountId}.", operation, accountId);
                return false;
            }

            var accountBalance = accountBalanceRepository.GetByAccountIdAndType(accountId, accountType);
            if (accountBalance == null)
            {
                logger.LogWarning("Attempted to {Operation} for account {AccountId} which does not have a {TypeName} balance.", operation, accountId, typeName);
                return false;
            }

            if (!isDeposit && accountBalance.Balance < amount)
            {
                logger.LogWarning("Attempted to {Operation} {Amount} from {TypeName} for account {AccountId} but only {CurrentBalance} is available.", operation, amount, typeName, accountId, accountBalance.Balance);
                return false;
            }

            accountBalance.Balance += isDeposit ? amount : -amount;
            accountBalanceRepository.Update(accountBalance);
            accountBalanceRepository.SaveChanges();

            logger.LogInformation("{OperationVerb} {Amount} {Direction} {AccountType} for account {AccountId}. New balance: {NewBalance}.",
                isDeposit ? "Deposited" : "Withdrew", amount, isDeposit ? "to" : "from", typeName, accountId, accountBalance.Balance);
            return true;
        }
    }
}