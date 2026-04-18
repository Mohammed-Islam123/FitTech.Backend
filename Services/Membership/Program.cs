using System.Diagnostics;
using System.Security.AccessControl;
using Carter;
using FluentValidation;
using Membership.Common.Security;
using Membership.Coommon.Behaviours;
using Membership.Domain;
using Membership.Features.Members.CreateMember;
using Membership.Infrastructure;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();

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

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddOpenApi(opt =>
{
    opt.AddFluentValidationRules();
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MembershipDbContext>();
    await context.Database.MigrateAsync();
}

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
