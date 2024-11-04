using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.User.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.LearningState.Service.Models;

namespace KnowledgeApp.LearningState.Service.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly IRepository<UserModel> _repository;

        public UserCreatedConsumer(IRepository<UserModel> repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            var message = context.Message;
            var userModel = await _repository.GetAsync(message.UserId);

            if (userModel != null)
            {
                await context.RespondAsync(new UserCreatedResponse(
                    UserId: message.UserId,
                    IsSuccessful: false,
                    Message: "User already exists."
                ));
                return;
            }

            try
            {
                userModel = new UserModel
                {
                    Id = message.UserId,
                    UserName = message.UserName
                };

                await _repository.CreateAsync(userModel);

                await context.RespondAsync(new UserCreatedResponse(
                    UserId: message.UserId,
                    IsSuccessful: true,
                    Message: "User created successfully."
                ));
            }
            catch
            {
                await context.RespondAsync(new UserCreatedResponse(
                    UserId: message.UserId,
                    IsSuccessful: false,
                    Message: "Failed to create user in LearningState service."
                ));
            }
        }

    }
}