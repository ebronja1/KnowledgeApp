namespace KnowledgeApp.User.Contracts
{
    public record UserCreated(Guid UserId, string UserName);

    public record UserCreatedResponse(Guid UserId, bool IsSuccessful, string Message);

    public record UserUpdated(Guid UserId, string UpdatedUserName);

    public record UserUpdatedResponse(Guid UserId, bool IsSuccessful, string Message);

    public record UserDeleted(Guid UserId);

    public record UserDeletedResponse(Guid UserId, bool IsSuccessful, string Message);
}
