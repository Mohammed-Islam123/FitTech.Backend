using Activity.Common.Behaviours;
using Activity.Common.Security;
using Activity.Domain;
using Activity.Infrastructure;
using Activity.Infrastructure.Auth;
using Activity.Infrastructure.Seed;
using Carter;
using FluentValidation;
using JasperFx.Core.Reflection;
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

builder.AddNpgsqlDbContext<ActivityDbContext>(connectionName: "activityDb");
builder.Services.AddCarter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();

builder.Host.UseWolverine(opts =>
{
    opts.Discovery
        .IncludeAssembly(typeof(Shared.Events.AttendanceMarkedEvent).Assembly);

    opts.UseRabbitMqUsingNamedConnection("rabbitmq")
        .UseConventionalRouting(conventions =>
        {
            conventions.IncludeTypes(type => type.IsInNamespace("Shared.Events"));

            conventions.IncludeTypes(type => type.Name.EndsWith("Event"));
            var serviceName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            conventions.QueueNameForListener(type => $"{serviceName}-{type.FullName}");

        })
        .AutoProvision();

    opts.Policies.DisableConventionalLocalRouting();
});


var identityUrl = builder.Configuration["IDENTITY_API_HTTP"]
?? builder.Configuration["services:identity-api:http:0"]

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

builder.Services.AddHttpClient("IdentityAuth", c =>
    c.BaseAddress = new Uri("http://identity-api"));

builder.Services.AddTransient<ServiceTokenHandler>();

builder.Services.AddRefitClient<IMembershipServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://membership-api"))
    .AddHttpMessageHandler<ServiceTokenHandler>();

builder.Services.AddRefitClient<ICoursesServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://courses-api"))
    .AddHttpMessageHandler<ServiceTokenHandler>();

builder.Services.AddFluentValidationRulesToOpenApi();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
builder.Services.AddScoped<ActivitySeeder>();
builder.Services.AddActivityServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ActivityDbContext>();
    await context.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<ActivitySeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.MapDefaultEndpoints();
app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Activity API").WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

app.Run();
