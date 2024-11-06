using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.MassTransit;

namespace KnowledgeApp.Common.Telemetry
{
    public static class TelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, string serviceName)
        {
            // Configure OpenTelemetry with necessary instrumentation
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddRedisInstrumentation()
                        .AddMassTransitInstrumentation();
                    
                    tracing.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri("http://jaeger:4317"); 
                    });
                });

            return services;
        }
    }
}
