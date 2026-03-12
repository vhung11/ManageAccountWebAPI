namespace ManageAccountWebAPI.Data.DTOs
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal SavingsBalance { get; set; }
        public decimal CheckingBalance { get; set; }
        public decimal TotalBalance => SavingsBalance + CheckingBalance;
    }
}