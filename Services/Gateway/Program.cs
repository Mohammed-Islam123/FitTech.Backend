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


var swaggerConfig = YarpConfiguration.GetSwaggerConfig();

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

        options.AddDocument("Identity API", "/openapi/v1.json");
        options.AddDocument("Membership API", "/openapi/v1.json");
        options.AddDocument("Payment API", "/openapi/v1.json");
        options.AddDocument("Courses API", "/openapi/v1.json");
        options.AddDocument("Activity API", "/openapi/v1.json");
        options.AddDocument("Aggregation API", "/openapi/v1.json");
    }).RequireCors();

    app.Map("/swagger/{documentName}/swagger.json", () => { })
       .RequireCors();
}

app.MapDefaultEndpoints();
app.MapReverseProxy();
app.MapGet("/", () => Results.Redirect("/scalar", true));

app.Run();
