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
            var userModel = await _repository.GetAsync(message.UserId);

            if (userModel == null)
            {
                await context.RespondAsync(new UserDeletedResponse(
                    UserId: message.UserId,
                    IsSuccessful: false,
                    Message: "User not found."
                ));
                return;
            }

            try
            {
                await _repository.RemoveAsync(userModel.Id);

                await context.RespondAsync(new UserDeletedResponse(
                    UserId: message.UserId,
                    IsSuccessful: true,
                    Message: "User deleted successfully."
                ));
            }
            catch
            {
                await context.RespondAsync(new UserDeletedResponse(
                    UserId: message.UserId,
                    IsSuccessful: false,
                    Message: "Failed to delete user in LearningState service."
                ));
            }
        }

    }
}
