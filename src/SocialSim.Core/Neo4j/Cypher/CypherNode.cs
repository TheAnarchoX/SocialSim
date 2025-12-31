namespace SocialSim.Core.Neo4j.Cypher;

public readonly record struct CypherNode<TNode>(string Alias, string? Label = null, string? Properties = null) : ICypherFragment
{
    public string Render()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required.", nameof(Alias));
        }

        var label = Label ?? Neo4jCypherNaming.GetLabel(typeof(TNode));
        var labelPart = string.IsNullOrWhiteSpace(label) ? string.Empty : $":{label}";
        var propsPart = string.IsNullOrWhiteSpace(Properties) ? string.Empty : $" {{ {Properties} }}";
        return $"({Alias.Trim()}{labelPart}{propsPart})";
    }

    public override string ToString() => Render();
}
