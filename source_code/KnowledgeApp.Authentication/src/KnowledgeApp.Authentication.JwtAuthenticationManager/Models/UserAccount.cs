using KnowledgeApp.Common;

namespace KnowledgeApp.Authentication.JwtAuthenticationManager.Models
{
    public class UserAccount
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}