namespace SocialSim.Core.Neo4j.Cypher;

public sealed record CypherQuery(string Text, IReadOnlyDictionary<string, object?> Parameters);
