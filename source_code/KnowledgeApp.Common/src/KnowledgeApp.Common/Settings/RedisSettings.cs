namespace KnowledgeApp.Common.Settings
{
    public class RedisCacheSettings
    {
        public string Host { get; init; }
        public int Port { get; init; }
        public string ConnectionString => $"{Host}:{Port}";
    }
}
