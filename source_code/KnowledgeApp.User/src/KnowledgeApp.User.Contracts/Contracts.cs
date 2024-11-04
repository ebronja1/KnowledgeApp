namespace KnowledgeApp.User.Contracts
{
    public record UserCreated(Guid UserId, string UserName);

    public record UserCreatedResponse(Guid UserId, bool IsSuccessful, string Message);

    public record UserUpdated(Guid UserId, string UpdatedUserName, string OldUserName, string OldPassword, string OldRole);

    public record UserUpdatedResponse(Guid UserId, bool IsSuccessful, string Message, string? UpdatedUserName = null, string? OldUserName = null, string? OldPassword = null, string? OldRole = null);

    public record UserDeleted(Guid UserId);

    public record UserDeletedResponse(Guid UserId, bool IsSuccessful, string Message);
}
