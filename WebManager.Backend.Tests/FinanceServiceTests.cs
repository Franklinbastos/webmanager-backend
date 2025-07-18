using Xunit;
using Microsoft.EntityFrameworkCore;
using WebManager.Data;
using WebManager.Models;
using WebManager.Services;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WebManager.Backend.Tests
{
    public class FinanceServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task ProcessAllFixedFinancesForUser_GeneratesCorrectNumberOfFinances()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new FinanceService(context);

            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", Password = "hashedpassword" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.FixedFinances.Add(new FixedFinance { Id = 1, UserId = user.Id, Description = "Rent", Amount = 1000, Type = "expense", NumberOfMonths = 12, BillingDay = 1, StartDate = DateTime.Now, IsActive = true });
            context.FixedFinances.Add(new FixedFinance { Id = 2, UserId = user.Id, Description = "Subscription", Amount = 50, Type = "expense", NumberOfMonths = 6, BillingDay = 15, StartDate = DateTime.Now, IsActive = true });
            await context.SaveChangesAsync();

            // Act
            await service.ProcessAllFixedFinancesForUser(user.Id);

            // Assert
            Assert.Equal(18, context.Finances.Count()); // 12 + 6
        }

        [Fact]
        public async Task ProcessFixedFinanceUpdate_UpdatesCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new FinanceService(context);

            var user = new User { Id = 1, Name = "Test User", Email = "test@example.com", Password = "hashedpassword" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var fixedFinance = new FixedFinance { Id = 1, UserId = user.Id, Description = "Rent", Amount = 1000, Type = "expense", NumberOfMonths = 12, BillingDay = 1, StartDate = DateTime.Now, IsActive = true };
            context.FixedFinances.Add(fixedFinance);
            await context.SaveChangesAsync();

            // Initial generation
            await service.ProcessFixedFinanceUpdate(fixedFinance.Id);
            Assert.Equal(12, context.Finances.Count());

            // Act - Update NumberOfMonths
            fixedFinance.NumberOfMonths = 8;
            context.FixedFinances.Update(fixedFinance);
            await context.SaveChangesAsync();

            await service.ProcessFixedFinanceUpdate(fixedFinance.Id);

            // Assert
            Assert.Equal(8, context.Finances.Count());
            // Verify dates and other properties if needed
        }
    }
}