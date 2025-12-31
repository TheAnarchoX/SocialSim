namespace SocialSim.Core.Neo4j.Repositories;

public sealed class Neo4jRelationshipRepositoryOptions
{
    public string FromKeyProperty { get; init; } = "Id";

    public string ToKeyProperty { get; init; } = "Id";

    public string WeightProperty { get; init; } = "Weight";

    public string? Database { get; init; }
}
