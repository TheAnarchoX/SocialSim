namespace SocialSim.Core.Neo4j.Repositories;

public interface INeo4jRelationshipRepository<TRelationship, TFromNode, TToNode>
{
    Task<TRelationship> CreateAsync(
        object fromKey,
        object toKey,
        TRelationship relationship,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TRelationship>> CreateBatchAsync(
        IEnumerable<(object FromKey, object ToKey, TRelationship Relationship)> relationships,
        CancellationToken cancellationToken = default);

    Task<TRelationship?> UpdateAsync(
        object fromKey,
        object toKey,
        TRelationship relationship,
        CancellationToken cancellationToken = default);

    Task<bool> SetWeightAsync(
        object fromKey,
        object toKey,
        double weight,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        object fromKey,
        object toKey,
        CancellationToken cancellationToken = default);

    Task<long> DeleteBatchAsync(
        IEnumerable<(object FromKey, object ToKey)> relationships,
        CancellationToken cancellationToken = default);

    Task<int?> GetDegreeOfSeparationAsync(
        object fromKey,
        object toKey,
        int maxHops = 6,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<object>?> GetShortestPathAsync(
        object fromKey,
        object toKey,
        int maxHops = 6,
        CancellationToken cancellationToken = default);
}
