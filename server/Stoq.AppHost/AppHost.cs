var builder = DistributedApplication.CreateBuilder(args);

// ─── Infrastructure ───────────────────────────────────────────────────────────
// PostgreSQL — one server, five named databases (one per module schema).
// Each module's Infrastructure project receives its connection string via Aspire
// service discovery — no hard-coded connection strings anywhere in the solution.
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var dbIdentity  = postgres.AddDatabase("stoq-identity");
var dbCatalog   = postgres.AddDatabase("stoq-catalog");
var dbInventory = postgres.AddDatabase("stoq-inventory");
var dbShopping  = postgres.AddDatabase("stoq-shopping");
var dbRetailer  = postgres.AddDatabase("stoq-retailer");

// RabbitMQ — used by MassTransit 8.x for all async inter-module integration events.
// The management UI is available in the Aspire dashboard sidebar when running locally.
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// ─── Application services ─────────────────────────────────────────────────────
var apiService = builder.AddProject<Projects.Stoq_ApiService>("apiservice")
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

builder.Build().Run();
