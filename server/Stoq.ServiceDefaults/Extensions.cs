using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string _healthEndpointPath = "/health";
    private const string _alivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Polly retry + circuit-breaker on every HttpClient by default.
            http.AddStandardResilienceHandler();

            // Resolve service names via Aspire service discovery.
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(builder.Environment.ApplicationName)

                    // Domain-level spans emitted by use cases in each module.
                    // The ActivitySource is defined in Stoq.Shared.Kernel as StoqActivitySource.
                    .AddSource("Stoq.*")

                    // Exclude health check endpoints to keep traces clean.
                    .AddAspNetCoreInstrumentation(options =>
                        options.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments(_healthEndpointPath)
                            && !ctx.Request.Path.StartsWithSegments(_alivenessEndpointPath))

                    .AddHttpClientInstrumentation()

                    // EF Core — captures DB operations end-to-end.
                    .AddEntityFrameworkCoreInstrumentation()

                    // Npgsql emits traces under the "Npgsql" activity source name.
                    .AddSource("Npgsql")

                    // MassTransit publishes spans for every message send/receive/consume.
                    .AddSource("MassTransit");
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for the app to be considered ready.
            app.MapHealthChecks(_healthEndpointPath);

            // Only checks tagged "live" must pass for the app to be considered alive.
            app.MapHealthChecks(_alivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
            });
        }

        return app;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // Local development: Aspire dashboard collects OTLP data automatically when
        // OTEL_EXPORTER_OTLP_ENDPOINT is set (injected by the AppHost).
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Production: export to Azure Monitor / Application Insights when the
        // connection string is present (set via Azure App Service application settings).
        // In Azure.Monitor.OpenTelemetry.AspNetCore 1.x, UseAzureMonitor() extends
        // OpenTelemetryBuilder and reads APPLICATIONINSIGHTS_CONNECTION_STRING automatically.
        if (!string.IsNullOrEmpty(
                builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }

        return builder;
    }
}
