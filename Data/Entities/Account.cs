namespace ManageAccountWebAPI.Data.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        // Navigation property
        public ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();
    }
}