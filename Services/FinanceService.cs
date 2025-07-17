using WebManager.Data;
using WebManager.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace WebManager.Services
{
    public class FinanceService
    {
        private readonly AppDbContext _context;

        public FinanceService(AppDbContext context)
        {
            _context = context;
        }

        public void ProcessFixedFinances()
        {
            Console.WriteLine("FinanceService: Starting ProcessFixedFinances...");
            var fixedFinances = _context.FixedFinances.Where(f => f.IsActive).ToList();
            Console.WriteLine($"FinanceService: Found {fixedFinances.Count} active fixed finances.");

            int financesAddedToContext = 0;

            foreach (var fixedFinance in fixedFinances)
            {
                Console.WriteLine($"FinanceService: Processing fixed finance: {fixedFinance.Description} (ID: {fixedFinance.Id}), NumberOfMonths: {fixedFinance.NumberOfMonths}");
                for (int i = 0; i < fixedFinance.NumberOfMonths; i++)
                {
                    var transactionDate = fixedFinance.StartDate.AddMonths(i);
                    // Ensure billing day does not exceed days in month
                    var day = Math.Min(fixedFinance.BillingDay, DateTime.DaysInMonth(transactionDate.Year, transactionDate.Month));
                    var billingDate = new DateTime(transactionDate.Year, transactionDate.Month, day);

                    var newFinance = new Finance
                    {
                        UserId = fixedFinance.UserId,
                        Date = billingDate,
                        Description = fixedFinance.Description,
                        Amount = fixedFinance.Amount,
                        Type = fixedFinance.Type
                    };
                    _context.Finances.Add(newFinance);
                    financesAddedToContext++;
                    Console.WriteLine($"FinanceService: Added finance entry for '{fixedFinance.Description}' on {billingDate.ToShortDateString()}. Total added to context: {financesAddedToContext}");
                }
            }

            Console.WriteLine($"FinanceService: Total finance entries added to context before SaveChanges: {financesAddedToContext}");

            try
            {
                _context.SaveChanges();
                Console.WriteLine($"FinanceService: Saved changes. Total finance entries in DB after processing: {_context.Finances.Count()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FinanceService: Error saving changes: {ex.Message}");
                // Log inner exception if available
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"FinanceService: Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
