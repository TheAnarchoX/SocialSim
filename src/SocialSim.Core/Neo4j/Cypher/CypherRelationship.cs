namespace SocialSim.Core.Neo4j.Cypher;

public readonly record struct CypherRelationship<TRelationship>(
    string? Alias = null,
    string? Type = null,
    string? Properties = null,
    RelationshipDirection Direction = RelationshipDirection.LeftToRight,
    CypherRelationshipLength? Length = null) : ICypherFragment
{
    public string Render()
    {
        var type = Type ?? Neo4jCypherNaming.GetRelationshipType(typeof(TRelationship));

        var aliasPart = string.IsNullOrWhiteSpace(Alias) ? string.Empty : Alias.Trim();
        var typePart = string.IsNullOrWhiteSpace(type) ? string.Empty : $":{type}";
        var propsPart = string.IsNullOrWhiteSpace(Properties) ? string.Empty : $" {{ {Properties} }}";

        var lengthPart = Length is null ? string.Empty : Length.Value.Render();

        var inner = $"{aliasPart}{typePart}{propsPart}{lengthPart}";
        var bracket = $"[{inner}]";

        return Direction switch
        {
            RelationshipDirection.LeftToRight => $"-{bracket}->",
            RelationshipDirection.RightToLeft => $"<-{bracket}-",
            RelationshipDirection.Undirected => $"-{bracket}-",
            _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Unknown relationship direction."),
        };
    }

    public override string ToString() => Render();
}

public readonly record struct CypherRelationshipLength(int? MinHops = null, int? MaxHops = null)
{
    public string Render()
    {
        if (MinHops is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinHops), MinHops, "MinHops cannot be negative.");
        }

        if (MaxHops is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxHops), MaxHops, "MaxHops cannot be negative.");
        }

        if (MinHops is not null && MaxHops is not null && MinHops > MaxHops)
        {
            throw new ArgumentException("MinHops cannot be greater than MaxHops.");
        }

        // Neo4j variable-length syntax: * (any length), *min..max, *min.., *..max
        if (MinHops is null && MaxHops is null)
        {
            return "*";
        }

        var min = MinHops?.ToString() ?? string.Empty;
        var max = MaxHops?.ToString() ?? string.Empty;
        return $"*{min}..{max}";
    }

    public override string ToString() => Render();
}
