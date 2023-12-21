using Microsoft.EntityFrameworkCore;
using Order.Service.DataAccess.Entities;
using OrderEntity = Order.Service.DataAccess.Entities.Order;

namespace Order.Service.DataAccess.Contexts
{
    public class AppDbContext:DbContext
    {
        public AppDbContext( DbContextOptions<AppDbContext> options):base(options)
        {
            

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(Console.WriteLine, LogLevel.Information)
                .UseSnakeCaseNamingConvention();
        }

        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
