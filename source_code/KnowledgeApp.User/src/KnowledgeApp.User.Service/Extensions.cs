using KnowledgeApp.User.Service.Models;

namespace KnowledgeApp.User.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this UserModel user)
        {
            return new UserDto(user.Id, user.UserName, user.Password, user.Role);
        }
    }
}