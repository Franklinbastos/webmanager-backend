using WebManager.Data;
using WebManager.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; // Add this for async methods

namespace WebManager.Services
{
    public class FinanceService
    {
        private readonly AppDbContext _context;

        public FinanceService(AppDbContext context)
        {
            _context = context;
        }

        // New method to process all fixed finances for a given user
        public async Task ProcessAllFixedFinancesForUser(int userId)
        {
            Console.WriteLine($"FinanceService: Starting ProcessAllFixedFinancesForUser for User ID: {userId}");

            // 1. Delete ALL existing Finance entries for this user
            var allExistingFinancesForUser = await _context.Finances
                .Where(f => f.UserId == userId)
                .ToListAsync();

            _context.Finances.RemoveRange(allExistingFinancesForUser);
            Console.WriteLine($"FinanceService: Removed {allExistingFinancesForUser.Count} existing finance entries for User ID {userId}.");

            // 2. Re-generate and save ALL new Finance entries for ALL active FixedFinances of this user
            var allActiveFixedFinancesForUser = await _context.FixedFinances
                .Where(ff => ff.UserId == userId && ff.IsActive)
                .ToListAsync();

            foreach (var ff in allActiveFixedFinancesForUser)
            {
                for (int i = 0; i < ff.NumberOfMonths; i++)
                {
                    var transactionDate = ff.StartDate.AddMonths(i);
                    var day = Math.Min(ff.BillingDay, DateTime.DaysInMonth(transactionDate.Year, transactionDate.Month));
                    var billingDate = new DateTime(transactionDate.Year, transactionDate.Month, day);

                    var newFinance = new Finance
                    {
                        UserId = ff.UserId,
                        Date = billingDate,
                        Description = ff.Description,
                        Amount = ff.Amount,
                        Type = ff.Type,
                        FixedFinanceId = ff.Id // Assign the foreign key
                    };
                    _context.Finances.Add(newFinance);
                    Console.WriteLine($"FinanceService: Generated finance entry for '{ff.Description}' on {billingDate.ToShortDateString()} (FixedFinanceId: {ff.Id})");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"FinanceService: Saved changes. Total finance entries in DB for User ID {userId}: {_context.Finances.Count(f => f.UserId == userId)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FinanceService: Error saving changes for User ID {userId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"FinanceService: Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        // Refactored ProcessFixedFinanceUpdate to use the foreign key
        public async Task ProcessFixedFinanceUpdate(int fixedFinanceId)
        {
            Console.WriteLine($"FinanceService: Processing update for FixedFinance ID: {fixedFinanceId}");

            var fixedFinance = await _context.FixedFinances.FindAsync(fixedFinanceId);
            if (fixedFinance == null)
            {
                Console.WriteLine($"FinanceService: FixedFinance with ID {fixedFinanceId} not found.");
                return;
            }

            // 1. Delete existing Finance entries associated with this specific FixedFinance
            var existingFinancesToDelete = await _context.Finances
                .Where(f => f.FixedFinanceId == fixedFinanceId)
                .ToListAsync();

            _context.Finances.RemoveRange(existingFinancesToDelete);
            Console.WriteLine($"FinanceService: Removed {existingFinancesToDelete.Count} existing finance entries for FixedFinance ID {fixedFinanceId}.");

            // 2. Re-generate and save new Finance entries for this specific FixedFinance
            for (int i = 0; i < fixedFinance.NumberOfMonths; i++)
            {
                var transactionDate = fixedFinance.StartDate.AddMonths(i);
                var day = Math.Min(fixedFinance.BillingDay, DateTime.DaysInMonth(transactionDate.Year, transactionDate.Month));
                var billingDate = new DateTime(transactionDate.Year, transactionDate.Month, day);

                var newFinance = new Finance
                {
                    UserId = fixedFinance.UserId,
                    Date = billingDate,
                    Description = fixedFinance.Description,
                    Amount = fixedFinance.Amount,
                    Type = fixedFinance.Type,
                    FixedFinanceId = fixedFinance.Id // Assign the foreign key
                };
                _context.Finances.Add(newFinance);
                Console.WriteLine($"FinanceService: Re-generated finance entry for '{fixedFinance.Description}' on {billingDate.ToShortDateString()} (FixedFinanceId: {fixedFinance.Id})");
            }

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"FinanceService: Saved changes after updating FixedFinance ID {fixedFinanceId}. Total finance entries in DB: {_context.Finances.Count(f => f.FixedFinanceId == fixedFinanceId)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FinanceService: Error saving changes for FixedFinance ID {fixedFinanceId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"FinanceService: Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
