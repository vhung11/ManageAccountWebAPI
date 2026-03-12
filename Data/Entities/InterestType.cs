namespace ManageAccountWebAPI.Data.Entities
{
    public class InterestType
    {
        public int Id { get; set; }
        public decimal Rate { get; set; }

        // Navigation property
        public ICollection<AccountBalance>? AccountBalances { get; set; }
    }
}