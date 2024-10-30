using Microsoft.OpenApi.Models;
using KnowledgeApp.Paragraph.Service.Models;
using KnowledgeApp.Common.MassTransit;
using KnowledgeApp.Common.MongoDB;
using KnowledgeApp.Common.Settings;
using KnowledgeApp.Common.Redis;  // Redis extension

var builder = WebApplication.CreateBuilder(args);

// Load configuration settings
var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

// Add services to the container.
builder.Services.AddMongo()
    .AddMongoRepository<ParagraphModel>("Paragraph")
    .AddMassTransitWithRabbitMq()
    .AddRedisCacheService();
    
builder.Services.AddRedisCache();  // Use the Redis extension

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KnowledgeApp.Paragraph.Service", Version = "v1" });
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KnowledgeApp.Paragraph.Service v1"));
}

// Enable CORS
app.UseCors();

// Use HTTPS redirection and routing
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
