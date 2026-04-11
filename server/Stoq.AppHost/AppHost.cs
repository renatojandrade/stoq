IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// ─── Infrastructure ───────────────────────────────────────────────────────────
// PostgreSQL — one server, five named databases (one per module schema).
// Each module's Infrastructure project receives its connection string via Aspire
// service discovery — no hard-coded connection strings anywhere in the solution.
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
    .WithDataVolume("stoq-postgres-data")
    .WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> dbIdentity = postgres.AddDatabase("stoq-identity");
IResourceBuilder<PostgresDatabaseResource> dbCatalog = postgres.AddDatabase("stoq-catalog");
IResourceBuilder<PostgresDatabaseResource> dbInventory = postgres.AddDatabase("stoq-inventory");
IResourceBuilder<PostgresDatabaseResource> dbShopping = postgres.AddDatabase("stoq-shopping");
IResourceBuilder<PostgresDatabaseResource> dbRetailer = postgres.AddDatabase("stoq-retailer");

// RabbitMQ — used by MassTransit 8.x for all async inter-module integration events.
// The management UI is available in the Aspire dashboard sidebar when running locally.
IResourceBuilder<RabbitMQServerResource> rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume("stoq-rabbitmq-data")
    .WithManagementPlugin();

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
