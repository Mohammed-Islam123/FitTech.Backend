using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);


var mainDb = builder.AddPostgres("mainDbServer")
                     .WithLifetime(ContainerLifetime.Persistent)
                     .WithPgAdmin();




var identityDb = mainDb.AddDatabase("identity-db");

var membershipDb = mainDb.AddDatabase("membershipDb");

var paymentDb = mainDb.AddDatabase("paymentDb");

var coursesDb = mainDb.AddDatabase("coursesDb");

var activityDb = mainDb.AddDatabase("activityDb");

var aggregationDb = mainDb.AddDatabase("aggregationDb");

var chatDb = mainDb.AddDatabase("chatDb");

var rabbitUser = builder.AddParameter("rabbitmq-username");
var rabbitPass = builder.AddParameter("rabbitmq-password", secret: true);

var rabbit = builder.AddRabbitMQ("rabbitmq", userName: rabbitUser, password: rabbitPass)
                     .WithLifetime(ContainerLifetime.Persistent)
                    .WithManagementPlugin();

var smtpServer = builder.AddContainer("smtp4dev", "rnwood/smtp4dev")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(port: 5005, targetPort: 80)
    .WithEndpoint(port: 2525, targetPort: 25);

var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
                         .WithEndpoint("http", endpoint => endpoint.Port = 5051)
                         .WithEndpoint("https", endpoint => endpoint.Port = 7259)
                         .WithReference(identityDb)
                         .WithReference(rabbit)
                         .WaitFor(identityDb)
                         .WaitFor(rabbit);


var membershipApi = builder.AddProject<Projects.Membership>("membership-api")
                          .WithEndpoint("http", endpoint => endpoint.Port = 5121)
                          .WithEndpoint("https", endpoint => endpoint.Port = 7297)
                          .WithReference(membershipDb)
                          .WithReference(rabbit)
                          .WithReference(identityApi)
                          .WaitFor(membershipDb)
                          .WaitFor(rabbit);



var paymentApi = builder.AddProject<Projects.Payment>("payment-api")
    .WithEndpoint("http", endpoint => endpoint.Port = 5246)
    .WithEndpoint("https", endpoint => endpoint.Port = 7180)
    .WithReference(paymentDb)
    .WithReference(rabbit)
    .WithReference(identityApi)
    .WaitFor(paymentDb)
    .WaitFor(rabbit);

var coursesApi = builder.AddProject<Projects.Courses>("courses-api")
    .WithEndpoint("http", endpoint => endpoint.Port = 5101)
    .WithEndpoint("https", endpoint => endpoint.Port = 7181)
    .WithReference(coursesDb)
    .WithReference(rabbit)
    .WithReference(identityApi)
    .WaitFor(coursesDb)
    .WaitFor(rabbit);

var activityApi = builder.AddProject<Projects.Activity>("activity-api")
    .WithEndpoint("http", endpoint => endpoint.Port = 5102)
    .WithEndpoint("https", endpoint => endpoint.Port = 7102)
    .WithReference(activityDb)
    .WithReference(rabbit)
    .WithReference(identityApi)
    .WaitFor(activityDb)
    .WaitFor(rabbit);

var aggregationApi = builder.AddProject<Projects.Aggregation>("aggregation-api")
    .WithEndpoint("http", endpoint => endpoint.Port = 5103)
    .WithEndpoint("https", endpoint => endpoint.Port = 7103)
    .WithReference(aggregationDb)
    .WithReference(rabbit)
    .WaitFor(aggregationDb)
    .WaitFor(rabbit);

var notificationApi = builder.AddProject<Projects.Notification_Api>("notification-api")
       .WithReference(rabbit)
       .WithEnvironment("Email__Host", "localhost")
       .WithEnvironment("Email__Port", "2525")
       .WaitFor(rabbit)
       .WaitFor(smtpServer);

var chatApi = builder.AddProject<Projects.Chat>("chat-api")
                     .WithEndpoint("http", endpoint => endpoint.Port = 5274)
                     .WithEndpoint("https", endpoint => endpoint.Port = 7238)
                     .WithReference(chatDb)
                     .WithReference(rabbit)
                     .WithReference(identityApi)
                     .WaitFor(chatDb)
                     .WaitFor(rabbit);

var shopApi = builder.AddJavaApp(
    name: "shop-api",
    workingDirectory: "../../Services/Shop/",
    options: new JavaAppExecutableResourceOptions
    {
        OtelAgentPath = "../../FitTech.AppHost/FitTech.AppHost/agents"
    }
)
.WithEnvironment("SERVER_PORT", "5104")
.WithEnvironment("LOGGING_LEVEL_ORG_SPRINGFRAMEWORK_WEB", "DEBUG")
.WithHttpEndpoint(port: 5104, name: "http")
.WithHttpsEndpoint(port: 7104, name: "https");



var workoutLogsApi = builder.AddJavaApp(
    name: "workout-logs-api",
    workingDirectory: "../../Services/Workout-Logs/",
    options: new JavaAppExecutableResourceOptions
    {
        OtelAgentPath = "../../FitTech.AppHost/FitTech.AppHost/agents"
    }
)
.WithEnvironment("SERVER_PORT", "5105")
.WithHttpEndpoint(port: 5105, name: "http")
.WithHttpsEndpoint(port: 7105, name: "https");

var equipmentsApi = builder.AddJavaApp(
    name: "equipments-api",
    workingDirectory: "../../Services/Equipments/",
    options: new JavaAppExecutableResourceOptions
    {
        OtelAgentPath = "../../FitTech.AppHost/FitTech.AppHost/agents"
    }
)
.WithEnvironment("SERVER_PORT", "5106")
.WithHttpEndpoint(port: 5106, name: "http")
.WithHttpsEndpoint(port: 7106, name: "https");



var scalar = builder.AddScalarApiReference(options =>
{
    options.OpenApiRoutePattern = "openapi/{documentName}.json";
});

// Use WithApiReference to register the services
scalar.WithApiReference(identityApi)
      .WithApiReference(membershipApi)
      .WithApiReference(paymentApi)
      .WithApiReference(chatApi)
      .WithApiReference(coursesApi)
      .WithApiReference(activityApi)
      .WithApiReference(aggregationApi);

scalar.WaitFor(identityApi)
      .WaitFor(membershipApi)
      .WaitFor(paymentApi)
      .WaitFor(chatApi)
      .WaitFor(coursesApi)
      .WaitFor(activityApi)
      .WaitFor(aggregationApi)
      ;
builder.AddProject<Projects.Gateway>("gateway")
.WithEndpoint("http", endpoint =>
       {
           endpoint.Port = 5098;
           endpoint.TargetHost = "*";
       })
       .WithEndpoint("https", endpoint =>
       {
           endpoint.Port = 7248;
           endpoint.TargetHost = "*";
       })
       .WithReference(identityApi)
       .WithReference(membershipApi)
       .WithReference(paymentApi)
       .WithReference(coursesApi)
       .WithReference(activityApi)
       .WithReference(aggregationApi)
       .WithReference(chatApi)
       .WaitFor(identityApi)
       .WaitFor(membershipApi)
       .WaitFor(chatApi)
       .WaitFor(equipmentsApi)
       .WaitFor(shopApi)
       .WaitFor(workoutLogsApi);

builder.Build().Run();
