using Chat.Common;
using Chat.Consumers;
using Chat.Features.Conversations.GetOrCreate;
using Chat.Features.Conversations.ListConversations;
using Chat.Features.Messages.GetMessages;
using Chat.Features.Messages.SendMessage;
using Chat.Hubs;
using Chat.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<ChatDbContext>("chatDb");

var identityUrl = builder.Configuration["IDENTITY_API_HTTP"]
?? builder.Configuration["services:identity-api:http:0"]

    ?? "http://identity-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = identityUrl;
        opt.RequireHttpsMetadata = false;

        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://identity-api",
            ValidateAudience = false,
            ValidateLifetime = true,
            NameClaimType = "name",
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };

        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserAccessor>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq")!));

        cfg.ReceiveEndpoint("chat.user-created", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(ctx);
        });
    });
});

builder.AddPermissiveCors();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(opt =>
{
    opt.WithTitle("Chat API")
       .WithTheme(ScalarTheme.Mars);
});

app.UsePermissiveCors();
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
GetOrCreateConversationEndpoint.Map(api);
ListConversationsEndpoint.Map(api);
GetMessagesEndpoint.Map(api);
SendMessageEndpoint.Map(api);

app.MapHub<ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    await db.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

app.Run();
