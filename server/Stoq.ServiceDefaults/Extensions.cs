// Copyright (c) Stoq. All rights reserved.

using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Shared Aspire service defaults applied to every .NET project in the Stoq solution
/// via <c>builder.AddServiceDefaults()</c>. Registers OpenTelemetry (traces, metrics, logs),
/// health checks, HTTP client resilience, and service discovery.
/// </summary>
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// Applies all shared Aspire defaults to <paramref name="builder"/>.
    /// Call this once in every service's <c>Program.cs</c>.
    /// </summary>
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

    /// <summary>
    /// Configures the three OpenTelemetry pillars: tracing, metrics, and structured logging.
    /// Traces span the full call chain: HTTP → controller → MediatR → EF Core → MassTransit.
    /// </summary>
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
                            !ctx.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !ctx.Request.Path.StartsWithSegments(AlivenessEndpointPath))

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

    /// <summary>
    /// Adds the default liveness health check. Individual services add their own
    /// readiness checks (database connectivity, RabbitMQ, etc.) in their own <c>Program.cs</c>.
    /// </summary>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps the <c>/health</c> (readiness) and <c>/alive</c> (liveness) endpoints.
    /// Only exposed in Development to avoid information leakage in production.
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for the app to be considered ready.
            app.MapHealthChecks(HealthEndpointPath);

            // Only checks tagged "live" must pass for the app to be considered alive.
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
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
