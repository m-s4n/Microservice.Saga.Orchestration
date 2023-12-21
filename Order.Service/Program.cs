using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Service.Consumers;
using Order.Service.DataAccess.Contexts;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// AppDbContext IoC'a eklenir
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Baðlanmak istenilen RabbitMQ bilgisi verilir
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCompletedEventConsumer>();
    config.AddConsumer<OrderFailedEventConsumer>();

    config.UsingRabbitMq((context, _config) =>
    {
        _config.Host(builder.Configuration["RabbitMQ"]);

        // consumerlar hangi kuyruklarý dinliyor belirlenir
        _config.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue, 
            e => e.ConfigureConsumer<OrderCompletedEventConsumer>(context));

        _config.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue, 
            e => e.ConfigureConsumer<OrderFailedEventConsumer>(context));
    });

    
});

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();



app.Run();


