using Stoq.ApiService.Database;
using Stoq.ApiService.Services;
using Stoq.Catalog.Application;
using Stoq.Catalog.Infrastructure;
using Stoq.Identity.Application;
using Stoq.Identity.Infrastructure;
using Stoq.Inventory.Application;
using Stoq.Inventory.Infrastructure;
using Stoq.Shared.Kernel;
using Stoq.Shared.Kernel.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ─── Aspire service defaults (OTel, health checks, service discovery) ─────────
builder.AddServiceDefaults();

// ─── HTTP context & current-user service ─────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ─── Identity module — Application layer (MediatR, FluentValidation) ─────────
builder.Services.AddIdentityApplication();
builder.Services.AddSharedServices();

// ─── Identity module — Infrastructure (ASP.NET Identity, JWT, MassTransit) ───
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// ─── EF Core: IdentityModuleDbContext ─────────────────────────────────────────
// Aspire populates "stoq-identity" from service discovery at runtime.
// During local Aspire dev, the connection string is injected via environment variables.
var identityConnectionString = builder.Configuration.GetConnectionString("stoq-identity")
    ?? throw new InvalidOperationException("Connection string 'stoq-identity' is not configured.");

builder.Services.AddIdentityDbContext(identityConnectionString);

// ─── Catalog module — Application layer (MediatR, FluentValidation) ──────────
builder.Services.AddCatalogApplication();

// ─── Catalog module — Infrastructure (EF Core, repositories) ─────────────────
builder.Services.AddCatalogInfrastructure(builder.Configuration);

// ─── EF Core: CatalogModuleDbContext ──────────────────────────────────────────
var catalogConnectionString = builder.Configuration.GetConnectionString("stoq-catalog")
    ?? throw new InvalidOperationException("Connection string 'stoq-catalog' is not configured.");

builder.Services.AddCatalogDbContext(catalogConnectionString);

// ─── Inventory module — Application layer (MediatR, FluentValidation) ────────
builder.Services.AddInventoryApplication();

// ─── Inventory module — Infrastructure (EF Core, repositories) ───────────────
builder.Services.AddInventoryInfrastructure();

// ─── EF Core: InventoryModuleDbContext ────────────────────────────────────────
var inventoryConnectionString = builder.Configuration.GetConnectionString("stoq-inventory")
    ?? throw new InvalidOperationException("Connection string 'stoq-inventory' is not configured.");

builder.Services.AddInventoryDbContext(inventoryConnectionString);

// ─── Controllers & OpenAPI ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

// ─── Middleware pipeline ──────────────────────────────────────────────────────
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "ByCoders API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();

// ─── Apply EF Core migrations on startup (development only) ──────────────────
if (app.Environment.IsDevelopment())
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    await scope.MigrateModuleDatabasesAsync();
}

await app.RunAsync();
