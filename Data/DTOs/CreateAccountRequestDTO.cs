using System.ComponentModel.DataAnnotations;

namespace ManageAccountWebAPI.Data.DTOs
{
    public class CreateAccountRequestDTO
    {
        [Range(typeof(decimal), "0", "9999999999999999")]
        public decimal SavingsBalance { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999")]
        public decimal CheckingBalance { get; set; }
    }
}
