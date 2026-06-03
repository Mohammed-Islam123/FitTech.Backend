using Aggregation.Common.Security;
using Aggregation.Consumers;
using Aggregation.Domain;
using Aggregation.Features.Dashboard.GetAdminDashboard;
using Aggregation.Features.Dashboard.GetFinanceDashboard;
using Aggregation.Features.Reports.DownloadExcelReport;
using Aggregation.Infrastructure.Seed;
using Carter;
using JasperFx.Core.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPermissiveCors();

builder.AddNpgsqlDbContext<AggregationDbContext>(connectionName: "aggregationDb");
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
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
builder.Services.AddScoped<AggregationSeeder>();
builder.Services.AddScoped<GetAdminDashboardHandler>();
builder.Services.AddScoped<GetFinanceDashboardHandler>();
builder.Services.AddScoped<DownloadExcelReportHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AggregationDbContext>();
    await context.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<AggregationSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.MapDefaultEndpoints();
app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Aggregation API").WithTheme(ScalarTheme.Mars);
});
app.MapCarter();

app.Run();
