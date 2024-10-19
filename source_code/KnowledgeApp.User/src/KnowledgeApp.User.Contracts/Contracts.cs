namespace KnowledgeApp.User.Contracts
{
    public record UserCreated(Guid Id, string UserName);
    public record UserUpdated(Guid Id, string UserName);
    public record UserDeleted(Guid Id);
}
