using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using KnowledgeApp.Common;
using KnowledgeApp.Common.Telemetry; 

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

builder.Services.AddOpenTelemetryTracing("api-gateway");

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Configure Ocelot middleware
await app.UseOcelot();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

