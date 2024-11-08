using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using KnowledgeApp.Common.Settings;
using Microsoft.Extensions.Hosting;
using Serilog.Events;

namespace KnowledgeApp.Common.Logging
{
    public static class SerilogExtensions
    {
        public static IHostBuilder UseSerilogLogging(this IHostBuilder builder)
        {
            builder.UseSerilog((context, services, configuration) =>
            {
                // Retrieve configuration
                var config = services.GetRequiredService<IConfiguration>();
                
                // Retrieve settings from configuration
                var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

                // Configure Serilog to write logs to MSSQL Server
                configuration
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)  // Adjust to the desired level
                    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()) // Console output in JSON format
                    .WriteTo.Seq("http://seq:5341") // Seq logging for structured logs
                    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "log.txt"); // Log to file in JSON format
            });

            return builder;
        }
    }
}
