using ManageAccountWebAPI.Data.Constants;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Interfaces;

namespace ManageAccountWebAPI.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IAccountBalanceRepository accountBalanceRepository;
        private readonly IInterestTypeRepository interestTypeRepository;
        private readonly ILogger<TransactionService> logger;

        public TransactionService(IAccountRepository accountRepository, IAccountBalanceRepository accountBalanceRepository, IInterestTypeRepository interestTypeRepository, ILogger<TransactionService> logger)
        {
            this.accountRepository = accountRepository;
            this.accountBalanceRepository = accountBalanceRepository;
            this.interestTypeRepository = interestTypeRepository;
            this.logger = logger;
        }

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

        public decimal WithdrawAllCheckingBalance(int accountId)
        {
            var account = accountRepository.GetById(accountId);
            if (account == null)
            {
                logger.LogWarning("Attempted to withdraw all checking balance for non-existent account with id {AccountId}.", accountId);
                return 0;
            }

            var accountBalance = accountBalanceRepository.GetByAccountIdAndType(accountId, AccountType.Checking);
            if (accountBalance == null)
            {
                logger.LogWarning("Attempted to withdraw all checking balance for account {AccountId} which does not have a Checking balance.", accountId);
                return 0;
            }

            var withdrawnAmount = accountBalance.Balance;
            accountBalance.Balance = 0;
            accountBalanceRepository.Update(accountBalance);
            accountBalanceRepository.SaveChanges();

            logger.LogInformation("Withdrew all checking balance {Amount} from account {AccountId}. New balance: {NewBalance}.", withdrawnAmount, accountId, accountBalance.Balance);
            return withdrawnAmount;
        }

        public (int UpdatedBalanceCount, decimal TotalInterestApplied) ApplyInterestToAllAccounts()
        {
            var balances = accountBalanceRepository.GetAll().ToList();
            if (balances.Count == 0)
            {
                logger.LogInformation("Skipped applying interest because there are no account balances.");
                return (0, 0);
            }

            var interestRates = new Dictionary<int, decimal>();
            var updatedBalanceCount = 0;
            decimal totalInterestApplied = 0;

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
                totalInterestApplied += interestAmount;
            }

            if (updatedBalanceCount > 0)
            {
                accountBalanceRepository.SaveChanges();
            }

            logger.LogInformation("Applied interest to {UpdatedBalanceCount} balances. Total interest applied: {TotalInterestApplied}.", updatedBalanceCount, totalInterestApplied);
            return (updatedBalanceCount, totalInterestApplied);
        }
    }
}
