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

            var userModel = await _repository.GetAsync(message.Id);

            if (userModel != null)
            {
                return;
            }

            userModel = new UserModel
            {
                Id = message.Id,
                UserName = message.UserName
            };

            await _repository.CreateAsync(userModel);
        }
    }
}