using Scalar.AspNetCore;
using NetEscapades.Configuration.Yaml;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddYamlFile("gateway.yaml", optional: false, reloadOnChange: true);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapReverseProxy();

app.MapScalarApiReference(options =>
{
    options.Title = "FitTech API";
    options.Theme = ScalarTheme.DeepSpace;
    options.AddDocument("Identity API", "/docs/identity/openapi/v1.json");
});

app.Run();