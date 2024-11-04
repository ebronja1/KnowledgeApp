using System;
using System.Threading.Tasks;
using MassTransit;
using KnowledgeApp.User.Contracts;
using KnowledgeApp.Common;
using KnowledgeApp.User.Service.Models;

namespace KnowledgeApp.User.Service.Consumers
{
    public class UserUpdatedResponseConsumer : IConsumer<UserUpdatedResponse>
    {
        private readonly IRepository<UserModel> _userRepository;

        public UserUpdatedResponseConsumer(IRepository<UserModel> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Consume(ConsumeContext<UserUpdatedResponse> context)
        {
            var response = context.Message;

            if (!response.IsSuccessful)
            {
                var user = await _userRepository.GetAsync(response.UserId);
                if (user != null)
                {
                    user.UserName = response.OldUserName;
                    user.Password = response.OldPassword;
                    user.Role = response.OldRole;
                    await _userRepository.UpdateAsync(user);

                    Console.WriteLine($"User update failed for ID: {response.UserId}. Message: {response.Message}");
                }
            }
            else
            {
                Console.WriteLine($"User successfully updated for ID: {response.UserId}");
            }
        }
    }
}

