namespace SocialSim.Core.Neo4j.Repositories;

public interface INeo4jNodeRepository<TNode>
{
    Task<TNode> CreateAsync(TNode node, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TNode>> CreateBatchAsync(IEnumerable<TNode> nodes, CancellationToken cancellationToken = default);

    Task<TNode?> GetByKeyAsync(object key, CancellationToken cancellationToken = default);

    Task<TNode?> UpdateAsync(object key, TNode node, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TNode>> UpdateBatchAsync(
        IEnumerable<(object Key, TNode Node)> updates,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(object key, CancellationToken cancellationToken = default);

    Task<long> DeleteBatchAsync(IEnumerable<object> keys, CancellationToken cancellationToken = default);
}
