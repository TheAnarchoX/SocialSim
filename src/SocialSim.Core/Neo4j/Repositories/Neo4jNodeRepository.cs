using System.Text.Json;
using Neo4j.Driver;
using SocialSim.Core.Neo4j.Cypher;

namespace SocialSim.Core.Neo4j.Repositories;

public sealed class Neo4jNodeRepository<TNode> : INeo4jNodeRepository<TNode>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDriver _driver;
    private readonly Neo4jNodeRepositoryOptions _options;
    private readonly string _label;

    public Neo4jNodeRepository(IDriver driver, Neo4jNodeRepositoryOptions? options = null)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? new Neo4jNodeRepositoryOptions();
        _label = Neo4jCypherNaming.GetLabel(typeof(TNode));

        if (string.IsNullOrWhiteSpace(_options.KeyProperty))
        {
            throw new ArgumentException("KeyProperty is required.", nameof(options));
        }
    }

    public async Task<TNode> CreateAsync(TNode node, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(node);

        var props = ToPropertyMap(node);

        var query = new CypherQueryBuilder()
            .Create($"(n:{_label})")
            .Set("n = $props")
            .Return("n")
            .WithParam("props", props)
            .Build();

        var created = await ExecuteWriteSingleNodeAsync(query, cancellationToken).ConfigureAwait(false);
        return created ?? throw new InvalidOperationException("Neo4j CREATE did not return a node.");
    }

    public async Task<IReadOnlyList<TNode>> CreateBatchAsync(IEnumerable<TNode> nodes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var rows = nodes.Select(ToPropertyMap).ToList();
        if (rows.Count == 0)
        {
            return Array.Empty<TNode>();
        }

        var query = new CypherQuery(
            $"UNWIND $rows AS row CREATE (n:{_label}) SET n = row RETURN n",
            new Dictionary<string, object?> { ["rows"] = rows });

        await using var session = CreateSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var created = new List<TNode>(rows.Count);
            while (await cursor.FetchAsync().ConfigureAwait(false))
            {
                created.Add(FromNode(cursor.Current["n"].As<INode>()));
            }

            return created;
        }).ConfigureAwait(false);
    }

    public Task<TNode?> GetByKeyAsync(object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var query = new CypherQueryBuilder()
            .Match($"(n:{_label} {{{_options.KeyProperty}: $key}})")
            .Return("n")
            .Limit(1)
            .WithParam("key", key)
            .Build();

        return ExecuteReadSingleNodeAsync(query, cancellationToken);
    }

    public Task<TNode?> UpdateAsync(object key, TNode node, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(node);

        var props = ToPropertyMap(node);
        props[_options.KeyProperty] = key;

        var query = new CypherQueryBuilder()
            .Match($"(n:{_label} {{{_options.KeyProperty}: $key}})")
            .Set("n = $props")
            .Return("n")
            .Limit(1)
            .WithParam("key", key)
            .WithParam("props", props)
            .Build();

        return ExecuteWriteSingleNodeAsync(query, cancellationToken);
    }

    public async Task<IReadOnlyList<TNode>> UpdateBatchAsync(
        IEnumerable<(object Key, TNode Node)> updates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var rows = new List<Dictionary<string, object?>>() ;
        foreach (var (key, node) in updates)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(node);

            var props = ToPropertyMap(node);
            props[_options.KeyProperty] = key;
            rows.Add(props);
        }

        if (rows.Count == 0)
        {
            return Array.Empty<TNode>();
        }

        var keyProperty = _options.KeyProperty;
        var query = new CypherQuery(
            $"UNWIND $rows AS row MERGE (n:{_label} {{{keyProperty}: row.{keyProperty}}}) SET n = row RETURN n",
            new Dictionary<string, object?> { ["rows"] = rows });

        await using var session = CreateSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var updated = new List<TNode>(rows.Count);
            while (await cursor.FetchAsync().ConfigureAwait(false))
            {
                updated.Add(FromNode(cursor.Current["n"].As<INode>()));
            }

            return updated;
        }).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(object key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var query = new CypherQueryBuilder()
            .Match($"(n:{_label} {{{_options.KeyProperty}: $key}})")
            .Delete("n")
            .Return("count(n) AS deleted")
            .WithParam("key", key)
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

    public async Task<long> DeleteBatchAsync(IEnumerable<object> keys, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var keyList = keys.ToList();
        if (keyList.Count == 0)
        {
            return 0;
        }

        var keyProperty = _options.KeyProperty;
        var query = new CypherQuery(
            $"UNWIND $keys AS key MATCH (n:{_label} {{{keyProperty}: key}}) WITH collect(n) AS nodes FOREACH (n IN nodes | DETACH DELETE n) RETURN size(nodes) AS deleted",
            new Dictionary<string, object?> { ["keys"] = keyList });

        await using var session = CreateSession();
        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleAsync().ConfigureAwait(false);
            return record["deleted"].As<long>();
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

    private async Task<TNode?> ExecuteReadSingleNodeAsync(CypherQuery query, CancellationToken cancellationToken)
    {
        await using var session = CreateSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query.Text, ToDriverParams(query.Parameters)).ConfigureAwait(false);
            var record = await cursor.SingleOrDefaultAsync().ConfigureAwait(false);
            if (record is null)
            {
                return default;
            }

            return FromNode(record["n"].As<INode>());
        }).ConfigureAwait(false);
    }

    private async Task<TNode?> ExecuteWriteSingleNodeAsync(CypherQuery query, CancellationToken cancellationToken)
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

            return FromNode(record["n"].As<INode>());
        }).ConfigureAwait(false);
    }

    private static Dictionary<string, object?> ToPropertyMap(TNode node)
    {
        var json = JsonSerializer.Serialize(node, JsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions);
        return dict ?? new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    private static TNode FromNode(INode node)
    {
        var json = JsonSerializer.Serialize(node.Properties, JsonOptions);
        var value = JsonSerializer.Deserialize<TNode>(json, JsonOptions);
        if (value is null)
        {
            throw new InvalidOperationException($"Unable to deserialize Neo4j node properties into {typeof(TNode).Name}.");
        }

        return value;
    }

    private static IDictionary<string, object?> ToDriverParams(IReadOnlyDictionary<string, object?> parameters)
        => parameters.Count == 0
            ? new Dictionary<string, object?>(0)
            : parameters.ToDictionary(static kv => kv.Key, static kv => kv.Value);
}
