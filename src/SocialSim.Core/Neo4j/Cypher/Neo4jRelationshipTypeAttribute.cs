namespace SocialSim.Core.Neo4j.Cypher;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class Neo4jRelationshipTypeAttribute(string type) : Attribute
{
    public string Type { get; } = string.IsNullOrWhiteSpace(type)
        ? throw new ArgumentException("Relationship type is required.", nameof(type))
        : type.Trim();
}
