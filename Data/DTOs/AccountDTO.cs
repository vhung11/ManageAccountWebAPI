namespace ManageAccountWebAPI.Data.DTOs
{
    public class AccountDTO
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public int AccountId { get; set; }
        public decimal SavingsBalance { get; set; }
        public decimal CheckingBalance { get; set; }
        public decimal TotalBalance => SavingsBalance + CheckingBalance;
    }
}