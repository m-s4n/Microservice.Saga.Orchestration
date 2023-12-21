using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Service.StateDbContexts;
using Orchestration.Service.StateInstances;
using Orchestration.Service.StateMachines;
using Shared.Settings;

var builder = Host.CreateApplicationBuilder(args);

// Baðlanmak istenilen RabbitMQ bilgisi verilir
builder.Services.AddMassTransit(config =>
{
    config
    .AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
    .EntityFrameworkRepository(options =>
    {
        options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
        {
            _builder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
        });
    });

    config.UsingRabbitMq((context, _config) =>
    {
        _config.Host(builder.Configuration["RabbitMQ"]);

        _config.ReceiveEndpoint(RabbitMQSettings.StateMachineQueue, e => e.ConfigureSaga<OrderStateInstance>(context));
    });
});


var host = builder.Build();
host.Run();
