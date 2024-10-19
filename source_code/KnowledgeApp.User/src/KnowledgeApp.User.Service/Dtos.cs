using System.ComponentModel.DataAnnotations;

namespace KnowledgeApp.User.Service
{
    public record UserDto(Guid Id, string UserName, string Password);
    public record UserCreateDto([Required] string UserName, [Required] string Password);
    public record UserUpdateDto(string UserName, string Password);
}