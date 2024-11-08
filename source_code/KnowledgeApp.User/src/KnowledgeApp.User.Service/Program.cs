using Microsoft.OpenApi.Models;
using KnowledgeApp.User.Service.Models;
using KnowledgeApp.Common.MassTransit;
using KnowledgeApp.Common.MongoDB;
using KnowledgeApp.Common.Settings;
using KnowledgeApp.Authentication.JwtAuthenticationManager;
using KnowledgeApp.Common.Telemetry;
using Serilog;  // Redis extension;
using KnowledgeApp.Common.Logging;  // Redis extension;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Load configuration settings
    var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

    // Add services to the container.
    builder.Services.AddMongo()
        .AddMongoRepository<UserModel>("User")
        .AddMassTransitWithRabbitMq()
        .AddOpenTelemetryTracing("User");

    builder.Services.AddSingleton<JwtTokenHandler>();

    builder.Services.AddControllers(options =>
    {
        options.SuppressAsyncSuffixInActionNames = false;
    });

    builder.Services.AddCustomJwtAuthentication();

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();

    builder.Host.UseSerilogLogging();
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "KnowledgeApp.User.Service", Version = "v1" });
    });

    // Configure CORS
    var allowedOrigin = builder.Configuration["AllowedOrigin"];

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            policyBuilder.WithOrigins(allowedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
    ;

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KnowledgeApp.User.Service v1"));
    }

    // Enable CORS
    app.UseCors();

    // Use HTTPS redirection and routing
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}