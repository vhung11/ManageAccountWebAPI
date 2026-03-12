using System.ComponentModel.DataAnnotations;

namespace ManageAccountWebAPI.Data.DTOs
{
    public class CreateAccountRequestDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be blank.")]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "9999999999999999")]
        public decimal SavingsBalance { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999")]
        public decimal CheckingBalance { get; set; }
    }
}
