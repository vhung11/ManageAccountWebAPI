using System.ComponentModel.DataAnnotations;

namespace ManageAccountWebAPI.Data.DTOs
{
    public class UpdateAccountRequestDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@".*\S.*", ErrorMessage = "Name must not be blank.")]
        public string Name { get; set; } = string.Empty;
    }
}