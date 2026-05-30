using MassTransit;
using Notification.Api.Consumers;
using Notification.Api.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<EmailConfirmationRequestedConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.ConfigureEndpoints(ctx);
    });
});

var host = builder.Build();
host.Run();