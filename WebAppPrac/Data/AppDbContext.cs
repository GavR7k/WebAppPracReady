using Microsoft.EntityFrameworkCore;
using WebAppPrac.Models;

namespace WebAppPrac.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AdminUser> AdminUsers { get; set; }

        public DbSet<Article> Articles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}
