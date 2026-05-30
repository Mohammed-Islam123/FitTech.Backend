using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Payment.Common.Security;
using Payment.Infrastructure;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Seed;
using Refit;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPermissiveCors();

builder.AddNpgsqlDbContext<PaymentDbContext>(connectionName: "paymentDb");
builder.Services.AddCarter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddPaymentServices();
builder.Services.AddPaymentGateway();

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("rabbitmq")
        .UseConventionalRouting()
        .AutoProvision();
    opts.Policies.DisableConventionalLocalRouting();
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

    options.AddPolicy("AdminOrMember", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Admin", "Member"));
});

builder.Services.AddRefitClient<IIdentityServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://identity-api"));

builder.Services.AddOpenApi();
builder.Services.AddScoped<PaymentSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await context.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<PaymentSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.MapDefaultEndpoints();
app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Payment API")
        .WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

app.Run();
