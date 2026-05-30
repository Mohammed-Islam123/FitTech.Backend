using Carter;
using FluentValidation;
using Membership.Common.Security;
using Membership.Coommon.Behaviours;
using Membership.Domain;
using Membership.Infrastructure;
using Membership.Infrastructure.Seed;
using Membership.Infrastructure.Auth;
using Membership.Features.Members.CreateMember;
using Membership.Features.Payments;
using MicroElements.AspNetCore.OpenApi.FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Refit;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPermissiveCors();

builder.AddNpgsqlDbContext<MembershipDbContext>(connectionName: "membershipDb");
builder.Services.AddCarter();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("rabbitmq")
        .UseConventionalRouting()
        .AutoProvision();
    opts.Policies.DisableConventionalLocalRouting();
    opts.Policies.AddMiddleware<ValidationBehavior>();
    opts.Discovery.IncludeType<PaymentConfirmedConsumer>();

});

var identityUrl = builder.Configuration["services:identity-api:http:0"]
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? "http://identity-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityUrl;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Admin"));

    options.AddPolicy("MemberOnly", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Member"));
});

builder.Services.AddHttpClient("IdentityAuth", c =>
    c.BaseAddress = new Uri("http://identity-api"));

builder.Services.AddTransient<ServiceTokenHandler>();

builder.Services.AddRefitClient<IIdentityServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://identity-api"))
    .AddHttpMessageHandler<ServiceTokenHandler>();

builder.Services.AddRefitClient<IPaymentServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://payment-api"))
    .AddHttpMessageHandler<ServiceTokenHandler>();

builder.Services.AddFluentValidationRulesToOpenApi();

builder.Services.AddOpenApi();
builder.Services.AddScoped<MembershipSeeder>();
builder.Services.AddMembershipServices();
builder.Services.AddScoped<PaymentConfirmedConsumer>();
var app = builder.Build();

app.MapDefaultEndpoints();
app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi().AllowAnonymous(); ;
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Membership API")
        .WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

await app.StartAsync();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MembershipDbContext>();
    await context.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<MembershipSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

await app.WaitForShutdownAsync();
