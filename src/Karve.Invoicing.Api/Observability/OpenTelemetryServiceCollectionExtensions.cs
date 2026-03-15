using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Karve.Invoicing.Api.Observability;

/// <summary>
/// OpenTelemetry registration helpers for the API host.
/// </summary>
public static class OpenTelemetryServiceCollectionExtensions
{
    /// <summary>
    /// Registers OpenTelemetry traces, metrics, and logs using a vendor-neutral setup.
    /// </summary>
    /// <param name="builder">Application builder.</param>
    public static void AddKarveOpenTelemetry(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(OpenTelemetryOptions.SectionName);
        var options = section.Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        var environmentName = string.IsNullOrWhiteSpace(options.Environment)
            ? builder.Environment.EnvironmentName
            : options.Environment;

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(options.ServiceName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environmentName,
                ["service.namespace"] = "Karve"
            });

        var telemetryBuilder = builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(options.ServiceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environmentName,
                    ["service.namespace"] = "Karve"
                }));

        if (options.EnableTraces)
        {
            telemetryBuilder.WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(KarveActivitySource.Name)
                    .AddAspNetCoreInstrumentation(traceOptions =>
                    {
                        traceOptions.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(traceOptions =>
                    {
                        traceOptions.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation();

                ConfigureTraceExporter(tracing, builder.Environment, options);
            });
        }

        if (options.EnableMetrics)
        {
            telemetryBuilder.WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                ConfigureMetricExporter(metrics, builder.Environment, options);
            });
        }

        if (options.EnableLogs)
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resourceBuilder);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.ParseStateValues = true;

                ConfigureLogExporter(logging, builder.Environment, options);
            });
        }
    }

    private static void ConfigureTraceExporter(
        TracerProviderBuilder tracing,
        IHostEnvironment environment,
        OpenTelemetryOptions options)
    {
        if (environment.IsDevelopment())
        {
            tracing.AddConsoleExporter();
            return;
        }

        if (Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            tracing.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
            return;
        }

        tracing.AddConsoleExporter();
    }

    private static void ConfigureMetricExporter(
        MeterProviderBuilder metrics,
        IHostEnvironment environment,
        OpenTelemetryOptions options)
    {
        if (environment.IsDevelopment())
        {
            metrics.AddConsoleExporter();
            return;
        }

        if (Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            metrics.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
            return;
        }

        metrics.AddConsoleExporter();
    }

    private static void ConfigureLogExporter(
        OpenTelemetryLoggerOptions logging,
        IHostEnvironment environment,
        OpenTelemetryOptions options)
    {
        if (environment.IsDevelopment())
        {
            logging.AddConsoleExporter();
            return;
        }

        if (Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            logging.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
            return;
        }

        logging.AddConsoleExporter();
    }
}
