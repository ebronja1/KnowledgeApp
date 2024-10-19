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

            var userModel = await _repository.GetAsync(message.Id);

            if (userModel == null)
            {
                userModel = new UserModel
                {
                    Id = message.Id,
                    UserName = message.UserName
                };

                await _repository.CreateAsync(userModel);
            }
            else
            {
                userModel.UserName = message.UserName;

                await _repository.UpdateAsync(userModel);
            }
        }
    }
}