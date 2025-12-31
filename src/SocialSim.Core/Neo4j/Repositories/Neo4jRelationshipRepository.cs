using System.Text.Json;
using Neo4j.Driver;
using SocialSim.Core.Neo4j.Cypher;

namespace SocialSim.Core.Neo4j.Repositories;

public sealed class Neo4jRelationshipRepository<TRelationship, TFromNode, TToNode>
    : INeo4jRelationshipRepository<TRelationship, TFromNode, TToNode>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDriver _driver;
    private readonly Neo4jRelationshipRepositoryOptions _options;
    private readonly string _fromLabel;
    private readonly string _toLabel;
    private readonly string _relationshipType;

    public Neo4jRelationshipRepository(IDriver driver, Neo4jRelationshipRepositoryOptions? options = null)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? new Neo4jRelationshipRepositoryOptions();

        _fromLabel = Neo4jCypherNaming.GetLabel(typeof(TFromNode));
        _toLabel = Neo4jCypherNaming.GetLabel(typeof(TToNode));
        _relationshipType = Neo4jCypherNaming.GetRelationshipType(typeof(TRelationship));

        if (string.IsNullOrWhiteSpace(_options.FromKeyProperty))
        {
            throw new ArgumentException("FromKeyProperty is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.ToKeyProperty))
        {
            throw new ArgumentException("ToKeyProperty is required.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.WeightProperty))
        {
            throw new ArgumentException("WeightProperty is required.", nameof(options));
        }
    }

    public async Task<TRelationship> CreateAsync(
        object fromKey,
        object toKey,
        TRelationship relationship,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromKey);
        ArgumentNullException.ThrowIfNull(toKey);
        ArgumentNullException.ThrowIfNull(relationship);

        var props = ToPropertyMap(relationship);

        var query = new CypherQueryBuilder()
            .Match(
                $"(a:{_fromLabel} {{{_options.FromKeyProperty}: $fromKey}})",
                $"(b:{_toLabel} {{{_options.ToKeyProperty}: $toKey}})")
            .Merge($"(a)-[r:{_relationshipType}]->(b)")
            .Set("r = $props")
            .Return("r")
            .Limit(1)
            .WithParam("fromKey", fromKey)
            .WithParam("toKey", toKey)
            .WithParam("props", props)
            .Build();

        var created = await ExecuteWriteSingleRelationshipAsync(query, cancellationToken).ConfigureAwait(false);
        return created ?? throw new InvalidOperationException("Neo4j relationship CREATE did not return a relationship.");
    }

    public async Task<IReadOnlyList<TRelationship>> CreateBatchAsync(
        IEnumerable<(object FromKey, object ToKey, TRelationship Relationship)> relationships,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relationships);

        var rows = new List<Dictionary<string, object?>>();
        foreach (var (fromKey, toKey, relationship) in relationships)
        {
            ArgumentNullException.ThrowIfNull(fromKey);
            ArgumentNullException.ThrowIfNull(toKey);
            ArgumentNullException.ThrowIfNull(relationship);

            rows.Add(new Dictionary<string, object?>
            {
                ["fromKey"] = fromKey,
                ["toKey"] = toKey,
                ["props"] = ToPropertyMap(relationship)
            });
        }

        if (rows.Count == 0)
        {
            return Array.Empty<TRelationship>();
        }

        var query = new CypherQuery(
            $"UNWIND $rows AS row " +
            $"MATCH (a:{_fromLabel} {{{_options.FromKeyProperty}: row.fromKey}}) " +
            $"MATCH (b:{_toLabel} {{{_options.ToKeyProperty}: row.toKey}}) " +
            $"MERGE (a)-[r:{_relationshipType}]->(b) " +
            $"SET r = row.props " +
            $"RETURN r",
            new Dictionary<string, object?> { ["rows"] = rows });

        await using var session = CreateSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var created = new List<TRelationship>(rows.Count);
            while (await cursor.FetchAsync().ConfigureAwait(false))
            {
                created.Add(FromRelationship(cursor.Current["r"].As<IRelationship>()));
            }

            return created;
        }).ConfigureAwait(false);
    }

    public Task<TRelationship?> UpdateAsync(
        object fromKey,
        object toKey,
        TRelationship relationship,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromKey);
        ArgumentNullException.ThrowIfNull(toKey);
        ArgumentNullException.ThrowIfNull(relationship);

        var props = ToPropertyMap(relationship);

        var query = new CypherQueryBuilder()
            .Match($"(a:{_fromLabel} {{{_options.FromKeyProperty}: $fromKey}})-[r:{_relationshipType}]->(b:{_toLabel} {{{_options.ToKeyProperty}: $toKey}})")
            .Set("r += $props")
            .Return("r")
            .Limit(1)
            .WithParam("fromKey", fromKey)
            .WithParam("toKey", toKey)
            .WithParam("props", props)
            .Build();

        return ExecuteWriteSingleRelationshipAsync(query, cancellationToken);
    }

    public async Task<bool> SetWeightAsync(
        object fromKey,
        object toKey,
        double weight,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromKey);
        ArgumentNullException.ThrowIfNull(toKey);

        var query = new CypherQueryBuilder()
            .Match($"(a:{_fromLabel} {{{_options.FromKeyProperty}: $fromKey}})-[r:{_relationshipType}]->(b:{_toLabel} {{{_options.ToKeyProperty}: $toKey}})")
            .Set($"r.{_options.WeightProperty} = $weight")
            .Return("count(r) AS updated")
            .WithParam("fromKey", fromKey)
            .WithParam("toKey", toKey)
            .WithParam("weight", weight)
            .Build();

        await using var session = CreateSession();
        var updated = await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleAsync().ConfigureAwait(false);
            return record["updated"].As<long>();
        }).ConfigureAwait(false);

        return updated > 0;
    }

    public async Task<bool> DeleteAsync(
        object fromKey,
        object toKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromKey);
        ArgumentNullException.ThrowIfNull(toKey);

        var query = new CypherQueryBuilder()
            .Match($"(a:{_fromLabel} {{{_options.FromKeyProperty}: $fromKey}})-[r:{_relationshipType}]->(b:{_toLabel} {{{_options.ToKeyProperty}: $toKey}})")
            .Delete("r")
            .Return("count(r) AS deleted")
            .WithParam("fromKey", fromKey)
            .WithParam("toKey", toKey)
            .Build();

        await using var session = CreateSession();
        var deleted = await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleAsync().ConfigureAwait(false);
            return record["deleted"].As<long>();
        }).ConfigureAwait(false);

        return deleted > 0;
    }

    public async Task<long> DeleteBatchAsync(
        IEnumerable<(object FromKey, object ToKey)> relationships,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relationships);

        var rows = new List<Dictionary<string, object?>>();
        foreach (var (fromKey, toKey) in relationships)
        {
            ArgumentNullException.ThrowIfNull(fromKey);
            ArgumentNullException.ThrowIfNull(toKey);

            rows.Add(new Dictionary<string, object?>
            {
                ["fromKey"] = fromKey,
                ["toKey"] = toKey
            });
        }

        if (rows.Count == 0)
        {
            return 0;
        }

        var query = new CypherQuery(
            $"UNWIND $rows AS row " +
            $"MATCH (a:{_fromLabel} {{{_options.FromKeyProperty}: row.fromKey}})-[r:{_relationshipType}]->(b:{_toLabel} {{{_options.ToKeyProperty}: row.toKey}}) " +
            $"WITH collect(r) AS rels " +
            $"FOREACH (r IN rels | DELETE r) " +
            $"RETURN size(rels) AS deleted",
            new Dictionary<string, object?> { ["rows"] = rows });

        await using var session = CreateSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleAsync().ConfigureAwait(false);
            return record["deleted"].As<long>();
        }).ConfigureAwait(false);
    }

    public async Task<int?> GetDegreeOfSeparationAsync(
        object fromKey,
        object toKey,
        int maxHops = 6,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromKey);
        ArgumentNullException.ThrowIfNull(toKey);

        if (maxHops <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHops), maxHops, "maxHops must be greater than zero.");
        }

        var pathPattern =
            $"(a:{_fromLabel} {{{_options.FromKeyProperty}: $fromKey}})" +
            $"-[r:{_relationshipType}*..{maxHops}]->" +
            $"(b:{_toLabel} {{{_options.ToKeyProperty}: $toKey}})";

        var query = new CypherQueryBuilder()
            .Match(CypherPattern.Path("p", CypherPattern.ShortestPath(new CypherPattern(pathPattern))))
            .Return("length(p) AS hops")
            .Limit(1)
            .WithParam("fromKey", fromKey)
            .WithParam("toKey", toKey)
            .Build();

        await using var session = CreateSession();
        var hops = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleOrDefaultAsync().ConfigureAwait(false);
            if (record is null)
            {
                return (long?)null;
            }

            return record["hops"].As<long>();
        }).ConfigureAwait(false);

        if (hops is null)
        {
            return null;
        }

        return hops.Value > int.MaxValue ? int.MaxValue : (int)hops.Value;
    }

    public async Task<IReadOnlyList<object>?> GetShortestPathAsync(
        object fromKey,
        object toKey,
        int maxHops = 6,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromKey);
        ArgumentNullException.ThrowIfNull(toKey);

        if (maxHops <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHops), maxHops, "maxHops must be greater than zero.");
        }

        if (!string.Equals(_options.FromKeyProperty, _options.ToKeyProperty, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Shortest path key extraction requires FromKeyProperty and ToKeyProperty to be the same.");
        }

        var keyProperty = _options.FromKeyProperty;

        var pathPattern =
            $"(a:{_fromLabel} {{{_options.FromKeyProperty}: $fromKey}})" +
            $"-[r:{_relationshipType}*..{maxHops}]->" +
            $"(b:{_toLabel} {{{_options.ToKeyProperty}: $toKey}})";

        var query = new CypherQueryBuilder()
            .Match(CypherPattern.Path("p", CypherPattern.ShortestPath(new CypherPattern(pathPattern))))
            .Return($"[n IN nodes(p) | n.{keyProperty}] AS keys")
            .Limit(1)
            .WithParam("fromKey", fromKey)
            .WithParam("toKey", toKey)
            .Build();

        await using var session = CreateSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleOrDefaultAsync().ConfigureAwait(false);
            if (record is null)
            {
                return default(IReadOnlyList<object>);
            }

            var keys = record["keys"].As<List<object>>();
            return keys;
        }).ConfigureAwait(false);
    }

    private IAsyncSession CreateSession()
    {
        if (string.IsNullOrWhiteSpace(_options.Database))
        {
            return _driver.AsyncSession();
        }

        return _driver.AsyncSession(o => o.WithDatabase(_options.Database));
    }

    private async Task<TRelationship?> ExecuteWriteSingleRelationshipAsync(CypherQuery query, CancellationToken cancellationToken)
    {
        await using var session = CreateSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleOrDefaultAsync().ConfigureAwait(false);
            if (record is null)
            {
                return default;
            }

            return FromRelationship(record["r"].As<IRelationship>());
        }).ConfigureAwait(false);
    }

    private static Dictionary<string, object?> ToPropertyMap(TRelationship relationship)
    {
        var json = JsonSerializer.Serialize(relationship, JsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions);
        return dict ?? new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    private static TRelationship FromRelationship(IRelationship relationship)
    {
        var json = JsonSerializer.Serialize(relationship.Properties, JsonOptions);
        var value = JsonSerializer.Deserialize<TRelationship>(json, JsonOptions);
        if (value is null)
        {
            throw new InvalidOperationException(
                $"Unable to deserialize Neo4j relationship properties into {typeof(TRelationship).Name}.");
        }

        return value;
    }

    private static IDictionary<string, object?> ToDriverParams(IReadOnlyDictionary<string, object?> parameters)
        => parameters.Count == 0
            ? new Dictionary<string, object?>(0)
            : parameters.ToDictionary(static kv => kv.Key, static kv => kv.Value);
}
