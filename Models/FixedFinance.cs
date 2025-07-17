
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

        public int NumberOfMonths { get; set; } = 1; // How many months this fixed finance will repeat
        public int BillingDay { get; set; } = 1; // Day of the month (1-31) for billing/income

        public DateTime StartDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
