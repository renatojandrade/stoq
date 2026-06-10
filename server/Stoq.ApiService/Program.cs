using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Stoq.ApiService.Database;
using Stoq.ApiService.Hubs;
using Stoq.ApiService.Services;
using Stoq.Catalog.Application;
using Stoq.Catalog.Infrastructure;
using Stoq.Identity.Application;
using Stoq.Identity.Infrastructure;
using Stoq.Inventory.Application;
using Stoq.Inventory.Application.Ports;
using Stoq.Inventory.Infrastructure;
using Stoq.Shared.Kernel;
using Stoq.Shared.Kernel.Services;
using Stoq.Shopping.Application;
using Stoq.Shopping.Infrastructure;

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
// All module consumers are registered here so they share the single MassTransit bus.
builder.Services.AddIdentityInfrastructure(
    builder.Configuration,
    configureAdditionalConsumers: configurator =>
    {
        Stoq.Identity.Infrastructure.InfrastructureServiceExtensions.AddIdentityConsumers(configurator);
        Stoq.Shopping.Infrastructure.InfrastructureServiceExtensions.AddShoppingConsumers(configurator);
        var prefetchCount = builder.Configuration.GetValue<int?>("ReceiptParsing:GlobalPrefetchCount") ?? 10;
        Stoq.Inventory.Infrastructure.InfrastructureServiceExtensions.AddInventoryConsumers(configurator, prefetchCount);
        Stoq.Catalog.Infrastructure.InfrastructureServiceExtensions.AddCatalogConsumers(configurator);
    });

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
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// ─── EF Core: InventoryModuleDbContext ────────────────────────────────────────
var inventoryConnectionString = builder.Configuration.GetConnectionString("stoq-inventory")
    ?? throw new InvalidOperationException("Connection string 'stoq-inventory' is not configured.");

builder.Services.AddInventoryDbContext(inventoryConnectionString);

// ─── Shopping module — Application layer (MediatR, FluentValidation) ─────────
builder.Services.AddShoppingApplication();

// ─── Shopping module — Infrastructure (EF Core, repositories) ────────────────
builder.Services.AddShoppingInfrastructure();

// ─── EF Core: ShoppingModuleDbContext ─────────────────────────────────────────
var shoppingConnectionString = builder.Configuration.GetConnectionString("stoq-shopping")
    ?? throw new InvalidOperationException("Connection string 'stoq-shopping' is not configured.");

builder.Services.AddShoppingDbContext(shoppingConnectionString);

// ─── SignalR ──────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddScoped<IReceiptNotificationService, CompositeReceiptNotificationService>();
builder.Services.AddScoped<IHouseholdSyncService, HouseholdSyncService>();

// Allow JWT via query string for SignalR WebSocket connections.
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    Func<MessageReceivedContext, Task>? original = options.Events?.OnMessageReceived;
    options.Events ??= new JwtBearerEvents();
    options.Events.OnMessageReceived = async context =>
    {
        if (original is not null)
        {
            await original(context);
        }

        StringValues token = context.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(token) &&
            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Token = token;
        }
    };
});

// ─── Controllers & OpenAPI ────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
app.MapHub<ReceiptHub>("/hubs/receipts");
app.MapHub<HouseholdHub>("/hubs/household");
app.MapDefaultEndpoints();

// ─── Apply EF Core migrations on startup (development only) ──────────────────
if (app.Environment.IsDevelopment())
{
    await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
    await scope.MigrateModuleDatabasesAsync();
}

await app.RunAsync();
