using Microsoft.AspNetCore.Identity;
using KnowledgeApp.Common;

namespace KnowledgeApp.User.Service.Models
{
    public class UserModel : IModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}