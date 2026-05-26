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

var coursesApi = builder.AddProject<Projects.Courses>("courses-api")
    .WithReference(coursesDb)
    .WithReference(rabbit)
    .WithReference(identityApi)
    .WaitFor(coursesDb)
    .WaitFor(rabbit);

var activityApi = builder.AddProject<Projects.Activity>("activity-api")
    .WithReference(activityDb)
    .WithReference(rabbit)
    .WithReference(identityApi)
    .WaitFor(activityDb)
    .WaitFor(rabbit);

var aggregationApi = builder.AddProject<Projects.Aggregation>("aggregation-api")
    .WithReference(aggregationDb)
    .WithReference(rabbit)
    .WaitFor(aggregationDb)
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
      .WaitFor(aggregationApi);
builder.AddProject<Projects.Gateway>("gateway")
       .WithReference(identityApi)
       .WithReference(paymentApi)
       .WithReference(coursesApi)
       .WithReference(activityApi)
       .WithReference(aggregationApi)
       .WithReference(chatApi) 
       .WaitFor(identityApi)
       .WaitFor(membershipApi)
       .WaitFor(chatApi);

builder.Build().Run();
