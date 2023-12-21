using MassTransit;
using Payment.Service.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);



// Ba�lanmak istenilen RabbitMQ bilgisi verilir
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<PaymentStartedEventConsumer>();

    config.UsingRabbitMq((context, _config) =>
    {
        _config.Host(builder.Configuration["RabbitMQ"]);

        // consumerlar hangi kuyruklar� dinliyor belirlenir
        _config.ReceiveEndpoint(RabbitMQSettings.Payment_StartedEventQueue,
            e => e.ConfigureConsumer<PaymentStartedEventConsumer>(context));
    });

    

});

var app = builder.Build();



app.Run();

