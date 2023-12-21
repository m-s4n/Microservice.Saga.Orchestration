using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.Service.Consumers;
using Stock.Service.Services;
using StockEntity = Stock.Service.DataAccess.Entities.Stock;

var builder = WebApplication.CreateBuilder(args);



// Baðlanmak istenilen RabbitMQ bilgisi verilir
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedEventConsumer>();
    config.AddConsumer<StockRollbackMessageConsumer>();

    config.UsingRabbitMq((context, _config) =>
    {
        _config.Host(builder.Configuration["RabbitMQ"]);

        // consumerlar hangi kuyruklrý dinliyor belirlenir
        _config.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,
            e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));

        _config.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue,
            e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
    });

    
});

// MongoDB IoC'a eklenir
builder.Services.AddSingleton<MongoDbService>();


var app = builder.Build();

using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();

if(!await (await mongoDbService.GetCollection<StockEntity>().FindAsync(x => true)).AnyAsync())
{
    mongoDbService.GetCollection<StockEntity>().InsertOne(new()
    {
        ProductId = 1,
        Count = 200
    });
    mongoDbService.GetCollection<StockEntity>().InsertOne(new()
    {
        ProductId = 2,
        Count = 300
    });
    mongoDbService.GetCollection<StockEntity>().InsertOne(new()
    {
        ProductId = 3,
        Count = 50
    });
    mongoDbService.GetCollection<StockEntity>().InsertOne(new()
    {
        ProductId = 4,
        Count = 10
    });
    mongoDbService.GetCollection<StockEntity>().InsertOne(new()
    {
        ProductId = 5,
        Count = 60
    });
}



app.Run();

