using Microsoft.EntityFrameworkCore;
using WebManager.Models;

namespace WebManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Finance> Finances { get; set; }
        public DbSet<User> Users { get; set; } // Adicionado
    }
}
