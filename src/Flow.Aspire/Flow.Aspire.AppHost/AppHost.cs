using System.Reflection.Emit;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var username = builder.AddParameter("username", secret: true, value: "admin");
var password = builder.AddParameter("password", secret: true, value: "admin");

var keycloak = builder.AddKeycloak("keycloak", 8080 , username, password)
       .WithDataVolume("keycloak")
       .WithRealmImport("./Realms");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var postgresTransactionsApi = builder.AddPostgres("postgrestransactionsapiservice")
    .WithPgAdmin();
var transactionsApiDb = postgresTransactionsApi.AddDatabase("transactionsapiservicedb");

var postgresReportsApiService = builder.AddPostgres("postgresreportsapiservice")
    .WithPgAdmin();
var reportsApiDb = postgresReportsApiService.AddDatabase("reportsapiservicedb");


var transactionsApiService = builder.AddProject<Projects.Flow_Transactions_ApiService>("transactionsapiservice")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(transactionsApiDb)
    .WithReference(seq)
    .WaitFor(seq)
    .WithHttpHealthCheck("/health");

var reportsApiService = builder.AddProject<Projects.Flow_Reports_ApiService>("reportsapiservice")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(reportsApiDb)
    .WithReference(seq)
    .WaitFor(seq)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Flow_Web_Blazor>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(transactionsApiService)
    .WaitFor(transactionsApiService)
    .WithReference(reportsApiService)
    .WaitFor(reportsApiService);

builder.Build().Run();

