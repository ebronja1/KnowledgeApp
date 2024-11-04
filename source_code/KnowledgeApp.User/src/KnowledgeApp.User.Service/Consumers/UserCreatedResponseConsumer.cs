using System;
using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.User.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.User.Service.Models;
using Microsoft.Extensions.Logging;
using Polly;

public class UserCreatedResponseConsumer : IConsumer<UserCreatedResponse>
{
    private readonly IRepository<UserModel> _repository;
    private readonly ILogger<UserCreatedResponseConsumer> _logger;

    public UserCreatedResponseConsumer(IRepository<UserModel> repository, ILogger<UserCreatedResponseConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedResponse> context)
    {
        var response = context.Message;

        if (!response.IsSuccessful)
        {

            // Remove the user from the User database if the creation was unsuccessful
            var userToDelete = await _repository.GetAsync(response.UserId);
            if (userToDelete != null)
            {
                await _repository.RemoveAsync(userToDelete.Id);
                _logger.LogInformation($"Deleted user ID: {response.UserId} from database due to creation failure.");
            }

            return; // Exit early if the creation was unsuccessful
        }

        // If creation was successful
        _logger.LogInformation($"User created successfully for ID: {response.UserId}");
    }
}
