
using System.ComponentModel.DataAnnotations;

namespace WebManager.Models
{
    public class FixedFinance
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "The Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression("^(income|expense)$", ErrorMessage = "The Type must be either 'income' or 'expense'.")]
        public string Type { get; set; } = "expense"; // "income" ou "expense"

        public bool IsActive { get; set; } = true;
    }
}
