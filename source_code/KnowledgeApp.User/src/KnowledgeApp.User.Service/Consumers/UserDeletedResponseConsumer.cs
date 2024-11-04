using System;
using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.User.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.User.Service.Models;

namespace KnowledgeApp.User.Service.Consumers
{
    public class UserDeletedResponseConsumer : IConsumer<UserDeletedResponse>
    {
        private readonly IRepository<UserModel> _userRepository;

        public UserDeletedResponseConsumer(IRepository<UserModel> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Consume(ConsumeContext<UserDeletedResponse> context)
        {
            var response = context.Message;

            if (response.IsSuccessful)
            {
                var user = await _userRepository.GetAsync(response.UserId);
                if (user != null)
                {
                    await _userRepository.RemoveAsync(response.UserId);
                    Console.WriteLine($"User successfully deleted for ID: {response.UserId}");
                }
            }
            else
            {
                Console.WriteLine($"User deletion failed for ID: {response.UserId}. Message: {response.Message}");
                // Optionally, add additional actions here, like retry logic or error handling.
            }
        }
    }
}
