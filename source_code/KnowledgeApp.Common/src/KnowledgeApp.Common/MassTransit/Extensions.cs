using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KnowledgeApp.Common.Settings;

namespace KnowledgeApp.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(typeof(Extensions).Assembly);

                configure.UsingRabbitMq((context, configurator) =>
                {
                    // Retrieve configuration
                    var configuration = context.GetService<IConfiguration>();
                    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();

                    // Configure RabbitMQ host
                    configurator.Host(rabbitMQSettings.Host);

                    // Configure endpoints with KebabCase formatter
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

                    // Configure retry policy
                    const int retryCount = 3; // Number of retries
                    var retryInterval = TimeSpan.FromSeconds(5); // Interval between retries

                    configurator.UseMessageRetry(retryConfigurator =>
                    {
                        retryConfigurator.Interval(retryCount, retryInterval);
                    });
                });
            });

            services.AddMassTransitHostedService();

            return services;
        }
    }
}
