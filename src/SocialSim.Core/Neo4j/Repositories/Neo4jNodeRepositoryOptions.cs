namespace SocialSim.Core.Neo4j.Repositories;

public sealed class Neo4jNodeRepositoryOptions
{
    public string KeyProperty { get; init; } = "Id";

    public string? Database { get; init; }
}
