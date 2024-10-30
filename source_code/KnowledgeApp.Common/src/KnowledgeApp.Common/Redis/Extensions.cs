using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KnowledgeApp.Common.Settings;
using System;

namespace KnowledgeApp.Common.Redis
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            // Retrieve Redis settings from configuration
            var redisSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("RedisSettings").Get<RedisCacheSettings>();

            // Ensure the settings are correctly configured
            if (redisSettings == null || string.IsNullOrEmpty(redisSettings.Host) || redisSettings.Port <= 0)
            {
                throw new Exception("RedisCacheSettings are not configured correctly.");
            }

            // Add StackExchange Redis cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.ConnectionString; // Use the connection string property
                options.InstanceName = "RedisCache_";
            });

            return services;
        }

        public static IServiceCollection AddRedisCacheService(this IServiceCollection services)
        {
            services.AddSingleton<RedisCacheService>();

            return services;
        }
    }
}
