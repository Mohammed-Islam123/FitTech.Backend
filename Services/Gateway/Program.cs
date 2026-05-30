using Gateway;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.ReverseProxy.Swagger.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load YARP configuration from gateway.yaml into IConfiguration
builder.Configuration.AddYamlFile("gateway.yaml", optional: false, reloadOnChange: true);

builder.AddServiceDefaults();

// ── YARP reverse proxy ──────────────────────────────────────────────
// Routes and clusters are loaded from gateway.yaml (ReverseProxy section).
// The yaml file is copied to output and loaded via IConfiguration.
// Destinations use Aspire service discovery addresses like:
//   http+https://identity-api
// The .AddServiceDiscoveryDestinationResolver() call resolves these
// at runtime to the actual host:port (e.g. https://localhost:7259).
var swaggerConfig   = YarpConfiguration.GetSwaggerConfig();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddSwagger(swaggerConfig)
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddPermissiveCors();

// ── Auth ────────────────────────────────────────────────────────────
var identityUrl = builder.Configuration["services:identity-api:http:0"]
    ?? "http://identity-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityUrl;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddAuthorization();

// ── Middleware pipeline ──────────────────────────────────────────────
var app = builder.Build();

app.UsePermissiveCors();
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // ── Swagger / Scalar ────────────────────────────────────────────
    app.UseSwagger(options => options.PreSerializeFilters.Add((doc, req) =>
    {
        var newPaths = new OpenApiPaths();
        foreach (var path in doc.Paths)
        {
            var parts = path.Key.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                (parts[0], parts[1]) = (parts[1], parts[0]);
            newPaths.Add("/" + string.Join('/', parts), path.Value);
        }
        doc.Paths = newPaths;
    }));

    app.MapScalarApiReference(options =>
    {
        options.Title = "FitTech API Gateway";
        options.Theme = ScalarTheme.Mars;
        options
            .WithPersistentAuthentication()
            .AddHttpAuthentication("Bearer", scheme =>
            {
                scheme.Description = "Enter your JWT Bearer token";
            });

        options.OpenApiRoutePattern = "/swagger/FitTech-API/swagger.json";

        options.AddDocument("Identity API",    "/docs/identity/openapi/v1.json");
        options.AddDocument("Membership API",  "/docs/membership/openapi/v1.json");
        options.AddDocument("Payment API",     "/docs/payment/openapi/v1.json");
        options.AddDocument("Courses API",     "/docs/courses/openapi/v1.json");
        options.AddDocument("Activity API",    "/docs/activity/openapi/v1.json");
        options.AddDocument("Aggregation API", "/docs/aggregation/openapi/v1.json");
    }).RequireCors();

    app.Map("/swagger/{documentName}/swagger.json", () => { })
       .RequireCors();
}

app.MapDefaultEndpoints();
app.MapReverseProxy();
app.MapGet("/", () => Results.Redirect("/scalar", true));

app.Run();
