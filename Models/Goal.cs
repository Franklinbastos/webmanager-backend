
using System.ComponentModel.DataAnnotations;

namespace WebManager.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        public string Item { get; set; } = string.Empty;
        [Required]
        public decimal Value { get; set; }
        public string? Description { get; set; }
        public bool Completed { get; set; }
    }
}
