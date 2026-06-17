using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace BuildingBlocks.Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog with Console and Elasticsearch sinks.
    /// Reads ElasticConfiguration:Uri from configuration (appsettings or env vars).
    /// Call this on WebApplicationBuilder before Build().
    /// </summary>
    public static WebApplicationBuilder AddSerilogWithElasticsearch(
        this WebApplicationBuilder builder,
        string applicationName)
    {
        var environment = builder.Environment.EnvironmentName;
        var configuration = builder.Configuration;

        var elasticUri = configuration["ElasticConfiguration:Uri"]
                         ?? "http://localhost:9200";

        // Derive a safe index name: e.g. "catalog-api-development-2025-01"
        var indexFormat = $"{applicationName.ToLower().Replace(".", "-")}" +
                          $"-{environment.ToLower()}" +
                          $"-{DateTime.UtcNow:yyyy-MM}";

        builder.Host.UseSerilog((ctx, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("Assembly", Assembly.GetEntryAssembly()?.GetName().Name ?? applicationName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {ApplicationName} | {Message:lj}{NewLine}{Exception}")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                {
                    IndexFormat = indexFormat,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
                    NumberOfReplicas = 1,
                    NumberOfShards = 2,
                    FailureCallback = (logEvent, ex) =>
                        Console.WriteLine($"[Serilog] Elasticsearch sink failed: {ex?.Message}"),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog
                                       | EmitEventFailureHandling.WriteToFailureSink
                                       | EmitEventFailureHandling.RaiseCallback,
                    ModifyConnectionSettings = conn =>
                    {
                        conn.ServerCertificateValidationCallback((o, cert, chain, errors) => true);
                        return conn;
                    }
                });
        });

        return builder;
    }
}
