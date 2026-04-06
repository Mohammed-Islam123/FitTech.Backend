var builder = DistributedApplication.CreateBuilder(args);



// var identityDb = builder.AddPostgres("postgres")
//                      .WithLifetime(ContainerLifetime.Persistent)
//                         .AddDatabase("identity-db")
//                      ;
var membershipDb = builder.AddPostgres("membershipDb")
                     .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .AddDatabase("membership-db");

var rabbitUser = builder.AddParameter("rabbitmq-username");
var rabbitPass = builder.AddParameter("rabbitmq-password", secret: true);

var rabbit = builder.AddRabbitMQ("rabbitmq", userName: rabbitUser, password: rabbitPass)
                     .WithLifetime(ContainerLifetime.Persistent)
                    .WithManagementPlugin();

// var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
//                          .WithReference(identityDb)
//                          .WithReference(rabbit)
//                          .WaitFor(identityDb)
//                          .WaitFor(rabbit);


builder.AddProject<Projects.Membership>("membership-api")
                          .WithReference(membershipDb)
                          .WithReference(rabbit)
                          .WaitFor(membershipDb)
                          .WaitFor(rabbit);

builder.AddProject<Projects.Notification_Api>("notification-api")
       .WithReference(rabbit)
       .WaitFor(rabbit);

// builder.AddProject<Projects.Gateway>("gateway")
//        .WithReference(identityApi)
//        .WaitFor(identityApi);

builder.Build().Run();