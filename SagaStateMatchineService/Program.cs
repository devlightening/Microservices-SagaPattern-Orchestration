using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateMachines;
using SagaStateMatchineService;
using SagaStateMatchineService.StateDbContext;
using SagaStateMatchineService.StateInstances;

using Shared.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddSagaStateMachine<OrderStateMachine,OrderStateInstance>().
    EntityFrameworkRepository(options =>
    {
        options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
        {
            _builder.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));

        });
    });
    configurator.UsingRabbitMq((context, _configure) =>
    {

        _configure.Host(builder.Configuration["RabbitMQ"]);

        _configure.ReceiveEndpoint(RabbitMQSettings.StateMachineQueue, e => e.ConfigureSaga<OrderStateInstance>(context));

    }); 
});



var host = builder.Build();
host.Run();
