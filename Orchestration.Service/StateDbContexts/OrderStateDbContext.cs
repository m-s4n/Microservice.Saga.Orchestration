using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Orchestration.Service.StateMaps;


namespace Orchestration.Service.StateDbContexts
{
    public class OrderStateDbContext : SagaDbContext
    {
        // Saga Db context'in şemasına göre veritabanı oluşturulur.
        public OrderStateDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(Console.WriteLine, LogLevel.Information)
                .UseSnakeCaseNamingConvention();
        }

        // State'in validasyonunu ve bağlantılı bir şekilde state instance alınır
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new OrderStateMap();
            }
        }
    }
}
