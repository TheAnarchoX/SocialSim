# Neo4j Cypher Query Builder (Fluent API)

SocialSim uses Neo4j to store and query the social graph. This document describes the small fluent Cypher builder provided by `SocialSim.Core` for constructing Cypher text + parameters in a consistent way.

## Location

- C# implementation: `src/SocialSim.Core/Neo4j/Cypher/`

## What it does

- Builds a Cypher query string by composing common clauses (`MATCH`, `WHERE`, `RETURN`, etc.)
- Collects named parameters into a dictionary for use with the Neo4j .NET driver

## Optimization helpers

The builder includes a few lightweight helpers for common Neo4j query optimization controls:

- `Explain()` / `Profile()` to prefix the query with `EXPLAIN` / `PROFILE`.
- `WithCypherRuntime(...)`, `WithCypherPlanner(...)`, and `WithCypherOption(key, value)` to emit a `CYPHER ...` options header.
- `UsingIndex(...)`, `UsingScan(...)`, and `UsingJoinOn(...)` to attach `USING ...` hints to the most recent `MATCH` / `OPTIONAL MATCH` clause.

### Example: index hint + runtime

```csharp
using SocialSim.Core.Neo4j.Cypher;

[Neo4jLabel("User")]
file sealed class UserNode;

var a = new CypherNode<UserNode>("a");

var query = new CypherQueryBuilder()
    .WithCypherRuntime("slotted")
    .Match(a)
    .UsingIndex(a, "id")
    .Where("a.id = $agentId")
    .Return("a")
    .WithParam("agentId", agentId)
    .Build();

// CYPHER runtime=slotted
// MATCH (a:User)
// USING INDEX a:User(id)
// WHERE a.id = $agentId
// RETURN a
```

## What it does not do

- It does not validate Cypher syntax.
- It does not parse expression trees or validate property names (you still provide `WHERE`/property-map text).

## Example

```csharp
using SocialSim.Core.Neo4j.Cypher;

// Optional: annotate your domain types to control Neo4j labels/relationship types.
// If you don't, labels default to the CLR type name, and relationship types default to UPPER_SNAKE_CASE.

[Neo4jLabel("User")]
file sealed class UserNode;

[Neo4jRelationshipType("FOLLOWS")]
file sealed class FollowsRel;

var query = new CypherQueryBuilder()
    .Match(CypherPattern.Join(
        new CypherNode<UserNode>("a", Properties: "id: $agentId"),
        new CypherRelationship<FollowsRel>(Direction: RelationshipDirection.LeftToRight),
        new CypherNode<UserNode>("b")))
    .Where("a.IsActive = true")
    .Return("a")
    .WithParam("agentId", agentId)
    .Build();

// query.Text:
// MATCH (a:Agent { id: $agentId })
// WHERE a.IsActive = true
// RETURN a
//
// query.Parameters["agentId"] == agentId
```

## Complex pattern matching

The builder supports composing more complex `MATCH` patterns by:

- Passing multiple patterns to `Match(...)` / `OptionalMatch(...)` (they are joined with `, `).
- Building patterns with `CypherPattern.CommaSeparated(...)`, `CypherPattern.Group(...)`, and `CypherPattern.Path(...)`.
- Using variable-length relationships via `CypherRelationshipLength`.

### Multiple patterns in a single MATCH

```csharp
var query = new CypherQueryBuilder()
    .Match(
        CypherPattern.Join(
            new CypherNode<UserNode>("a"),
            new CypherRelationship<FollowsRel>(),
            new CypherNode<UserNode>("b")),
        CypherPattern.Join(
            new CypherNode<UserNode>("b"),
            new CypherRelationship<FollowsRel>(),
            new CypherNode<UserNode>("c")))
    .Return("a, b, c")
    .Build();

// MATCH (a:User)-[:FOLLOWS]->(b:User), (b:User)-[:FOLLOWS]->(c:User)
```

### Variable-length relationship (multi-hop traversal)

```csharp
var query = new CypherQueryBuilder()
    .Match(CypherPattern.Join(
        new CypherNode<UserNode>("a"),
        new CypherRelationship<FollowsRel>(Length: new CypherRelationshipLength(MinHops: 1, MaxHops: 3)),
        new CypherNode<UserNode>("b")))
    .Return("a, b")
    .Build();

// MATCH (a:User)-[:FOLLOWS*1..3]->(b:User)
```

### Path alias assignment

```csharp
var query = new CypherQueryBuilder()
    .Match(CypherPattern.Path("p", CypherPattern.Join(
        new CypherNode<UserNode>("a"),
        new CypherRelationship<FollowsRel>(Length: new CypherRelationshipLength()),
        new CypherNode<UserNode>("b"))))
    .Return("p")
    .Build();

// MATCH p = (a:User)-[:FOLLOWS*]->(b:User)
```

### Shortest path helpers

```csharp
var query = new CypherQueryBuilder()
    .Match(CypherPattern.Path("p",
        CypherPattern.ShortestPath(CypherPattern.Join(
            new CypherNode<UserNode>("a"),
            new CypherRelationship<FollowsRel>(Length: new CypherRelationshipLength(MaxHops: 5)),
            new CypherNode<UserNode>("b")))))
    .Return("p")
    .Build();

// MATCH p = shortestPath((a:User)-[:FOLLOWS*..5]->(b:User))
```
