namespace SocialSim.Core.Neo4j.Cypher;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class Neo4jLabelAttribute(string label) : Attribute
{
    public string Label { get; } = string.IsNullOrWhiteSpace(label)
        ? throw new ArgumentException("Label is required.", nameof(label))
        : label.Trim();
}
