using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.User.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service.Models;

namespace KnowledgeApp.LearningState.Service.Consumers
{
    public class UserUpdatedConsumer : IConsumer<UserUpdated>
    {
        private readonly IRepository<UserModel> _repository;

        public UserUpdatedConsumer(IRepository<UserModel> repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<UserUpdated> context)
        {
            var message = context.Message;
            var userModel = await _repository.GetAsync(message.UserId);

            if (userModel == null)
            {
                await context.RespondAsync(new UserUpdatedResponse(
                    UserId: message.UserId,
                    IsSuccessful: false,
                    Message: "User not found."
                ));
                return;
            }

            try
            {
                userModel.UserName = message.UpdatedUserName; // or other fields
                await _repository.UpdateAsync(userModel);

                await context.RespondAsync(new UserUpdatedResponse(
                    UserId: message.UserId,
                    IsSuccessful: true,
                    Message: "User updated successfully."
                ));
            }
            catch
            {
                await context.RespondAsync(new UserUpdatedResponse(
                    UserId: message.UserId,
                    IsSuccessful: false,
                    Message: "Failed to update user in LearningState service."
                ));
            }
        }

    }
}