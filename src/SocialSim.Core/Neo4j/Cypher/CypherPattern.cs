using System.Linq;
using System.Text;

namespace SocialSim.Core.Neo4j.Cypher;

public readonly record struct CypherPattern(string Text) : ICypherFragment
{
    public string Render() => Text;

    public override string ToString() => Text;

    public static CypherPattern Join(params ICypherFragment[] fragments)
    {
        ArgumentNullException.ThrowIfNull(fragments);

        var sb = new StringBuilder();
        foreach (var fragment in fragments)
        {
            if (fragment is null) throw new ArgumentException("Fragments cannot contain null values.", nameof(fragments));
            sb.Append(fragment.Render());
        }

        return new CypherPattern(sb.ToString());
    }

    public static CypherPattern CommaSeparated(params ICypherFragment[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Length == 0)
        {
            throw new ArgumentException("At least one pattern is required.", nameof(patterns));
        }

        var rendered = patterns
            .Select(p => p ?? throw new ArgumentException("Patterns cannot contain null values.", nameof(patterns)))
            .Select(p => p.Render());

        return new CypherPattern(string.Join(", ", rendered));
    }

    public static CypherPattern Group(ICypherFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        return new CypherPattern($"({fragment.Render()})");
    }

    public static CypherPattern Path(string alias, ICypherFragment pattern)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Alias is required.", nameof(alias));
        }

        ArgumentNullException.ThrowIfNull(pattern);
        return new CypherPattern($"{alias.Trim()} = {pattern.Render()}");
    }

    public static CypherPattern ShortestPath(ICypherFragment pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return new CypherPattern($"shortestPath({EnsureWrappedInParentheses(pattern.Render())})");
    }

    public static CypherPattern AllShortestPaths(ICypherFragment pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return new CypherPattern($"allShortestPaths({EnsureWrappedInParentheses(pattern.Render())})");
    }

    private static string EnsureWrappedInParentheses(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Pattern text is required.", nameof(text));
        }

        var trimmed = text.Trim();
        if (trimmed.Length >= 2 && trimmed[0] == '(' && trimmed[^1] == ')')
        {
            return trimmed;
        }

        return $"({trimmed})";
    }
}
