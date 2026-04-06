using System.Diagnostics;
using System.Security.AccessControl;
using Carter;
using FluentValidation;
using Membership.Coommon.Behaviours;
using Membership.Domain;
using Membership.Infrastructure;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Refit;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));
builder.AddNpgsqlDbContext<MembershipDbContext>(connectionName: "membershipDb");
builder.Services.AddCarter();
builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("rabbitmq")
        .UseConventionalRouting()
        .AutoProvision();
    // opts.PublishMessage<ExternalNotification>().ToRabbitExchange("custom-notifications");
    opts.Policies.DisableConventionalLocalRouting();
    opts.Policies.AddMiddleware<ValidationBehavior>();

});


builder.Services.AddRefitClient<IIdentityServiceClient>()
    .ConfigureHttpClient(c =>
        c.BaseAddress = new Uri(builder.Configuration.GetConnectionString("IdentityService")
            ?? throw new InvalidOperationException("IdentityService connection string is not configured.")));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();
builder.Services.AddOpenApi(opt =>
{
    opt.AddFluentValidationRules();
});
var app = builder.Build();

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Membership API")
        .WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

app.Run();





