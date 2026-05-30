using Carter;
using Courses.Common.Behaviours;
using Courses.Common.Security;
using Courses.Domain;
using Courses.Infrastructure;
using Courses.Infrastructure.Seed;
using FluentValidation;
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

builder.AddNpgsqlDbContext<CoursesDbContext>(connectionName: "coursesDb");
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

    options.AddPolicy("CoachOnly", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Coach"));

    options.AddPolicy("AdminOrCoach", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Admin", "Coach"));
});

builder.Services.AddHttpClient("IdentityAuth", c =>
    c.BaseAddress = new Uri("http://identity-api"));

builder.Services.AddTransient<Courses.Infrastructure.Auth.ServiceTokenHandler>();

builder.Services.AddRefitClient<IIdentityServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://identity-api"))
    .AddHttpMessageHandler<Courses.Infrastructure.Auth.ServiceTokenHandler>();

builder.Services.AddFluentValidationRulesToOpenApi();
builder.Services.AddOpenApi();
builder.Services.AddScoped<CoursesSeeder>();
builder.Services.AddCoursesServices();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Courses API")
        .WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

await app.StartAsync();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CoursesDbContext>();
    await context.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<CoursesSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

await app.WaitForShutdownAsync();
