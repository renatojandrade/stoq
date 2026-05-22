IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// ─── Stable credentials (stored in AppHost user secrets, never committed) ────
// Set once with:
//   dotnet user-secrets set "Parameters:postgres-password" "yourpassword" --project server/Stoq.AppHost
//   dotnet user-secrets set "Parameters:rabbitmq-password" "yourpassword" --project server/Stoq.AppHost
IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("postgres-password", secret: true);
IResourceBuilder<ParameterResource> rabbitmqPassword = builder.AddParameter("rabbitmq-password", secret: true);

// ─── Infrastructure ───────────────────────────────────────────────────────────
// PostgreSQL — one server, five named databases (one per module schema).
// ContainerLifetime.Persistent keeps the container alive after AppHost exits so
// Stoq.ApiService can be debugged directly from Visual Studio without Aspire.
// Fixed host port 5432 makes the connection string stable across restarts.
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume("stoq-postgres-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("tcp", e => { e.Port = 5432; e.IsProxied = false; });

IResourceBuilder<PostgresDatabaseResource> dbIdentity = postgres.AddDatabase("stoq-identity");
IResourceBuilder<PostgresDatabaseResource> dbCatalog = postgres.AddDatabase("stoq-catalog");
IResourceBuilder<PostgresDatabaseResource> dbInventory = postgres.AddDatabase("stoq-inventory");
IResourceBuilder<PostgresDatabaseResource> dbShopping = postgres.AddDatabase("stoq-shopping");
IResourceBuilder<PostgresDatabaseResource> dbRetailer = postgres.AddDatabase("stoq-retailer");

// RabbitMQ — used by MassTransit 8.x for all async inter-module integration events.
// ContainerLifetime.Persistent + fixed port 5672 mirrors the postgres approach above.
IResourceBuilder<RabbitMQServerResource> rabbitmq = builder.AddRabbitMQ("rabbitmq", password: rabbitmqPassword)
    .WithDataVolume("stoq-rabbitmq-data")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("tcp", e => { e.Port = 5672; e.IsProxied = false; })
    .WithEndpoint("management", e => { e.Port = 15672; e.IsProxied = false; });

// ─── Application services ─────────────────────────────────────────────────────
IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.Stoq_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(dbIdentity)
    .WithReference(dbCatalog)
    .WithReference(dbInventory)
    .WithReference(dbShopping)
    .WithReference(dbRetailer)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.Stoq_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync();
