namespace WebManager.Models
{
    public class Finance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = "expense"; // "income" ou "expense"
    }
}
