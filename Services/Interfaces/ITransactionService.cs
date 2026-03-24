namespace ManageAccountWebAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        public bool DepositToSavings(int accountId, decimal amount);
        public bool DepositToChecking(int accountId, decimal amount);
        public bool WithdrawFromSavings(int accountId, decimal amount);
        public bool WithdrawFromChecking(int accountId, decimal amount);
        public decimal GetTotalSavingsBalance();
        public decimal WithdrawAllCheckingBalance(int accountId);
    }
}