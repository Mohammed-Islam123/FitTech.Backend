using Gateway;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.ReverseProxy.Swagger.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var (routes, clusters, swaggerConfig) = YarpConfiguration.GetYarpConfiguration();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());

    options.AddPolicy("socketPolicy", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

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

builder.Services.AddReverseProxy()
    .LoadFromMemory(routes, clusters)
    .AddSwagger(swaggerConfig)
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
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
        options.WithTitle("FitTech API")
               .WithTheme(ScalarTheme.Mars)
               .WithPersistentAuthentication()
               .AddHttpAuthentication("Bearer", scheme =>
               {
                   scheme.Description = "Enter your JWT Bearer token";
               });

        options.OpenApiRoutePattern = "/swagger/FitTech-API/swagger.json";
    }).RequireCors();

    app.Map("/swagger/{documentName}/swagger.json", () => { })
       .RequireCors();
}

app.MapDefaultEndpoints();
app.MapReverseProxy();
app.MapGet("/", () => Results.Redirect("/scalar", true));

app.Run();