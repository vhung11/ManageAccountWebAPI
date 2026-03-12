namespace ManageAccountWebAPI.Data.Entities
{
    public class AccountBalance
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string? Type { get; set; }
        public decimal Balance { get; set; }
        public int InterestTypeId { get; set; }

        // Navigation properties
        public Account? Account { get; set; }
        public InterestType? InterestType { get; set; }
    }
}