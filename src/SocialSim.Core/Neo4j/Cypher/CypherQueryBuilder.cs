using System.Collections.ObjectModel;

namespace SocialSim.Core.Neo4j.Cypher;

public sealed class CypherQueryBuilder
{
    private readonly List<string> _clauses = new();
    private readonly Dictionary<string, object?> _parameters = new(StringComparer.Ordinal);
    private readonly List<string> _cypherOptions = new();
    private string? _queryMode;

    public CypherQueryBuilder Explain()
    {
        _queryMode = "EXPLAIN";
        return this;
    }

    public CypherQueryBuilder Profile()
    {
        _queryMode = "PROFILE";
        return this;
    }

    public CypherQueryBuilder WithCypherOption(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Option key is required.", nameof(key));
        }

        key = key.Trim();
        if (value is null)
        {
            _cypherOptions.Add(key);
            return this;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Option value cannot be empty when provided.", nameof(value));
        }

        _cypherOptions.Add($"{key}={value.Trim()}");
        return this;
    }

    public CypherQueryBuilder WithCypherRuntime(string runtime)
    {
        if (string.IsNullOrWhiteSpace(runtime))
        {
            throw new ArgumentException("Runtime is required.", nameof(runtime));
        }

        return WithCypherOption("runtime", runtime);
    }

    public CypherQueryBuilder WithCypherPlanner(string planner)
    {
        if (string.IsNullOrWhiteSpace(planner))
        {
            throw new ArgumentException("Planner is required.", nameof(planner));
        }

        return WithCypherOption("planner", planner);
    }

    public CypherQueryBuilder Match(string pattern)
    {
        _clauses.Add($"MATCH {pattern}");
        return this;
    }

    public CypherQueryBuilder UsingIndex(string variable, string label, params string[] properties)
    {
        if (string.IsNullOrWhiteSpace(variable))
        {
            throw new ArgumentException("Variable is required.", nameof(variable));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label is required.", nameof(label));
        }

        ArgumentNullException.ThrowIfNull(properties);
        if (properties.Length == 0)
        {
            throw new ArgumentException("At least one property is required.", nameof(properties));
        }

        var props = string.Join(", ", properties.Select(static p =>
        {
            if (string.IsNullOrWhiteSpace(p))
            {
                throw new ArgumentException("Property names cannot be empty.", nameof(properties));
            }

            return p.Trim();
        }));

        return AppendHintToLastMatch($"USING INDEX {variable.Trim()}:{label.Trim()}({props})");
    }

    public CypherQueryBuilder UsingIndex<TNode>(CypherNode<TNode> node, params string[] properties)
    {
        var label = node.Label ?? Neo4jCypherNaming.GetLabel(typeof(TNode));
        return UsingIndex(node.Alias, label, properties);
    }

    public CypherQueryBuilder UsingScan(string variable, string label)
    {
        if (string.IsNullOrWhiteSpace(variable))
        {
            throw new ArgumentException("Variable is required.", nameof(variable));
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label is required.", nameof(label));
        }

        return AppendHintToLastMatch($"USING SCAN {variable.Trim()}:{label.Trim()}");
    }

    public CypherQueryBuilder UsingScan<TNode>(CypherNode<TNode> node)
    {
        var label = node.Label ?? Neo4jCypherNaming.GetLabel(typeof(TNode));
        return UsingScan(node.Alias, label);
    }

    public CypherQueryBuilder UsingJoinOn(params string[] variables)
    {
        ArgumentNullException.ThrowIfNull(variables);
        if (variables.Length == 0)
        {
            throw new ArgumentException("At least one variable is required.", nameof(variables));
        }

        var vars = string.Join(", ", variables.Select(static v =>
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                throw new ArgumentException("Join variables cannot be empty.", nameof(variables));
            }

            return v.Trim();
        }));

        return AppendHintToLastMatch($"USING JOIN ON {vars}");
    }

    public CypherQueryBuilder Match(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Length == 0)
        {
            throw new ArgumentException("At least one pattern is required.", nameof(patterns));
        }

        return Match(string.Join(", ", patterns));
    }

    public CypherQueryBuilder Match(ICypherFragment pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Match(pattern.Render());
    }

    public CypherQueryBuilder Match(params ICypherFragment[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Length == 0)
        {
            throw new ArgumentException("At least one pattern is required.", nameof(patterns));
        }

        return Match(CypherPattern.CommaSeparated(patterns));
    }

    public CypherQueryBuilder OptionalMatch(string pattern)
    {
        _clauses.Add($"OPTIONAL MATCH {pattern}");
        return this;
    }

    public CypherQueryBuilder OptionalMatch(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Length == 0)
        {
            throw new ArgumentException("At least one pattern is required.", nameof(patterns));
        }

        return OptionalMatch(string.Join(", ", patterns));
    }

    public CypherQueryBuilder OptionalMatch(ICypherFragment pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return OptionalMatch(pattern.Render());
    }

    public CypherQueryBuilder OptionalMatch(params ICypherFragment[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Length == 0)
        {
            throw new ArgumentException("At least one pattern is required.", nameof(patterns));
        }

        return OptionalMatch(CypherPattern.CommaSeparated(patterns));
    }

    public CypherQueryBuilder Where(string predicate)
    {
        if (_clauses.Count > 0 && _clauses[^1].StartsWith("WHERE ", StringComparison.Ordinal))
        {
            _clauses[^1] = $"WHERE {predicate}";
            return this;
        }

        _clauses.Add($"WHERE {predicate}");
        return this;
    }

    public CypherQueryBuilder AndWhere(string predicate)
    {
        return AppendToWhereClause("AND", predicate);
    }

    public CypherQueryBuilder OrWhere(string predicate)
    {
        return AppendToWhereClause("OR", predicate);
    }

    public CypherQueryBuilder Create(string pattern)
    {
        _clauses.Add($"CREATE {pattern}");
        return this;
    }

    public CypherQueryBuilder Create(ICypherFragment pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Create(pattern.Render());
    }

    public CypherQueryBuilder Merge(string pattern)
    {
        _clauses.Add($"MERGE {pattern}");
        return this;
    }

    public CypherQueryBuilder Merge(ICypherFragment pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Merge(pattern.Render());
    }

    public CypherQueryBuilder Set(string expression)
    {
        _clauses.Add($"SET {expression}");
        return this;
    }

    public CypherQueryBuilder Delete(string expression)
    {
        _clauses.Add($"DELETE {expression}");
        return this;
    }

    public CypherQueryBuilder DetachDelete(string expression)
    {
        _clauses.Add($"DETACH DELETE {expression}");
        return this;
    }

    public CypherQueryBuilder With(string expression)
    {
        _clauses.Add($"WITH {expression}");
        return this;
    }

    public CypherQueryBuilder Return(string expression)
    {
        _clauses.Add($"RETURN {expression}");
        return this;
    }

    public CypherQueryBuilder OrderBy(string expression)
    {
        _clauses.Add($"ORDER BY {expression}");
        return this;
    }

    public CypherQueryBuilder Skip(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        _clauses.Add($"SKIP {count}");
        return this;
    }

    public CypherQueryBuilder Limit(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        _clauses.Add($"LIMIT {count}");
        return this;
    }

    public CypherQueryBuilder WithParam(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Parameter name is required.", nameof(name));
        }

        name = name.Trim();
        if (name.StartsWith('$'))
        {
            name = name[1..];
        }

        _parameters[name] = value;
        return this;
    }

    public CypherQuery Build()
    {
        var text = string.Join('\n', _clauses);

        if (_cypherOptions.Count > 0)
        {
            text = $"CYPHER {string.Join(' ', _cypherOptions)}\n{text}";
        }

        if (!string.IsNullOrWhiteSpace(_queryMode))
        {
            text = $"{_queryMode}\n{text}";
        }

        return new CypherQuery(text, new ReadOnlyDictionary<string, object?>(_parameters));
    }

    public override string ToString() => Build().Text;

    private CypherQueryBuilder AppendToWhereClause(string op, string predicate)
    {
        if (_clauses.Count > 0 && _clauses[^1].StartsWith("WHERE ", StringComparison.Ordinal))
        {
            _clauses[^1] = $"{_clauses[^1]} {op} {predicate}";
            return this;
        }

        _clauses.Add($"WHERE {predicate}");
        return this;
    }

    private CypherQueryBuilder AppendHintToLastMatch(string hint)
    {
        if (_clauses.Count == 0)
        {
            throw new InvalidOperationException("A Cypher hint must follow a MATCH/OPTIONAL MATCH clause.");
        }

        var last = _clauses[^1];
        if (!last.StartsWith("MATCH ", StringComparison.Ordinal) &&
            !last.StartsWith("OPTIONAL MATCH ", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("A Cypher hint must follow a MATCH/OPTIONAL MATCH clause.");
        }

        _clauses[^1] = $"{last} {hint}";
        return this;
    }
}
