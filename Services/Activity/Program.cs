using Activity.Common.Behaviours;
using Activity.Common.Security;
using Activity.Domain;
using Activity.Infrastructure;
using Carter;
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

builder.AddNpgsqlDbContext<ActivityDbContext>(connectionName: "activityDb");
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
        policy.RequireAuthenticatedUser().RequireRole("Admin"));
    options.AddPolicy("AdminOrCoach", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Admin", "Coach"));
    options.AddPolicy("AdminCoachOrMember", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Admin", "Coach", "Member"));
});

builder.Services.AddRefitClient<IMembershipServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://membership-api"));

builder.Services.AddRefitClient<ICoursesServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://courses-api"));

builder.Services.AddFluentValidationRulesToOpenApi();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ActivityDbContext>();
    await context.Database.MigrateAsync();
}

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Activity API").WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

app.Run();
