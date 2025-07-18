using Microsoft.EntityFrameworkCore;
using WebManager.Models;

namespace WebManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Finance> Finances { get; set; }
        public virtual DbSet<User> Users { get; set; } // Adicionado
        public virtual DbSet<Goal> Goals { get; set; }
        public virtual DbSet<FixedFinance> FixedFinances { get; set; }
    }
}
