using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using NetEscapades.Configuration.Yaml;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddYamlFile("gateway.yaml", optional: false, reloadOnChange: true);

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

builder.Services.AddAuthorization();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapScalarApiReference(options =>
{
    options.Title = "FitTech API Gateway";
    options.Theme = ScalarTheme.Mars;
    options.AddDocument("Identity API", "/docs/identity/openapi/v1.json");
    options.AddDocument("Membership API", "/docs/membership/openapi/v1.json");
    options.AddDocument("Payment API", "/docs/payment/openapi/v1.json");
    options.AddDocument("Courses API", "/docs/courses/openapi/v1.json");
    options.AddDocument("Activity API", "/docs/activity/openapi/v1.json");
    options.AddDocument("Aggregation API", "/docs/aggregation/openapi/v1.json");
});

app.Run();
