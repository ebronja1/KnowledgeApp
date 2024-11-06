using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using KnowledgeApp.Common.MassTransit;
using KnowledgeApp.Common.MongoDB;
using KnowledgeApp.LearningState.Service.Clients;
using KnowledgeApp.LearningState.Service.Models;
using Polly;
using Polly.Timeout;
using KnowledgeApp.Common.Telemetry;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configure services
builder.Services.AddMongo()
                .AddMongoRepository<LearningStateModel>("learning-states")
                .AddMongoRepository<ParagraphModel>("paragraphs")
                .AddMongoRepository<UserModel>("users")
                .AddMassTransitWithRabbitMq()
                .AddOpenTelemetryTracing("learning-states");

AddParagraphClient(builder.Services, configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KnowledgeApp.LearningState.Service", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KnowledgeApp.LearningState.Service v1"));

    app.UseCors(policy =>
    {
        policy.WithOrigins(configuration["AllowedOrigin"])
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddParagraphClient(IServiceCollection services, IConfiguration configuration)
{
    Random jitterer = new();

    services.AddHttpClient<ParagraphClient>(client =>
    {
        client.BaseAddress = new Uri(configuration["ParagraphService:BaseUrl"] ?? "https://localhost:5001");
    })
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt, _) =>
        {
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ParagraphClient>>();
            logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
        }
    ))
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ParagraphClient>>();
            logger.LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
        },
        onReset: () =>
        {
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ParagraphClient>>();
            logger.LogWarning($"Closing the circuit...");
        }
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}
