using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.User.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service.Models;
using Microsoft.Extensions.Logging;

namespace KnowledgeApp.LearningState.Service.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeleted>
    {
        private readonly IRepository<UserModel> _repository;
        private readonly ILogger<UserDeletedConsumer> _logger;

        public UserDeletedConsumer(IRepository<UserModel> repository, ILogger<UserDeletedConsumer> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<UserDeleted> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received UserDeleted event for User ID: {UserId}", message.Id);

            var userModel = await _repository.GetAsync(message.Id);
            if (userModel == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", message.Id);
                return;
            }

            try
            {
                await _repository.RemoveAsync(message.Id);
                _logger.LogInformation("Successfully removed User with ID: {UserId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing User with ID: {UserId}", message.Id);
                throw; // Rethrow the exception to allow MassTransit to handle retry policies
            }
        }
    }
}
