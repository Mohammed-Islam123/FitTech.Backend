using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);


var mainDb = builder.AddPostgres("mainDbServer")
                     .WithLifetime(ContainerLifetime.Persistent)
                     .WithPgAdmin();




var identityDb = mainDb.AddDatabase("identity-db");

var membershipDb = mainDb.AddDatabase("membershipDb");

var paymentDb = mainDb.AddDatabase("paymentDb");

var chatDb = mainDb.AddDatabase("chatDb");

var rabbitUser = builder.AddParameter("rabbitmq-username");
var rabbitPass = builder.AddParameter("rabbitmq-password", secret: true);

var rabbit = builder.AddRabbitMQ("rabbitmq", userName: rabbitUser, password: rabbitPass)
                     .WithLifetime(ContainerLifetime.Persistent)
                    .WithManagementPlugin();

var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
                         .WithReference(identityDb)
                         .WithReference(rabbit)
                         .WaitFor(identityDb)
                         .WaitFor(rabbit);


var membershipApi = builder.AddProject<Projects.Membership>("membership-api")
                          .WithReference(membershipDb)
                          .WithReference(rabbit)
                          .WithReference(identityApi)
                          .WaitFor(membershipDb)
                          .WaitFor(rabbit);



var paymentApi = builder.AddProject<Projects.Payment>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbit)
    .WithReference(identityApi)
    .WaitFor(paymentDb)
    .WaitFor(rabbit);

var notificationApi = builder.AddProject<Projects.Notification_Api>("notification-api")
       .WithReference(rabbit)
       .WaitFor(rabbit);


var chatApi = builder.AddProject<Projects.Chat>("chat-api")
                     .WithReference(chatDb)
                     .WithReference(rabbit)
                     .WaitFor(chatDb)
                     .WaitFor(rabbit);
var scalar = builder.AddScalarApiReference(options =>
{
    // Match this to what your APIs actually expose (default is openapi/v1.json)
    options.OpenApiRoutePattern = "openapi/{documentName}.json";
});

// Use WithApiReference to register the services
scalar.WithApiReference(identityApi)
      .WithApiReference(membershipApi)
      .WithApiReference(paymentApi)
      .WithApiReference(chatApi);

// Ensure the APIs are running before Scalar tries to scan them
scalar.WaitFor(identityApi)
      .WaitFor(membershipApi)
      .WaitFor(paymentApi)
      .WaitFor(chatApi);


builder.AddProject<Projects.Gateway>("gateway")
       .WithReference(identityApi)
       .WithReference(paymentApi)
       .WithReference(chatApi) 
       .WaitFor(identityApi)
       .WaitFor(membershipApi)
       .WaitFor(chatApi);

builder.Build().Run();
