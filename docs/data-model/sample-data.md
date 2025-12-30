# Sample Data

This document provides sample data for testing and development of SocialSim's social network.

## Overview

Sample datasets include:
- **Minimal**: 10 users, 50 posts - for unit tests
- **Small**: 100 users, 1K posts - for integration tests
- **Medium**: 10K users, 100K posts - for performance testing
- **Large**: 1M users, 50M posts - for scale testing (synthetic)

## Minimal Dataset

### Sample Users

```sql
INSERT INTO users (id, decentralized_id, handle, display_name, email, is_simulated, follower_count, following_count, created_at) VALUES
('00000000-0000-0000-0000-000000000001', 'did:plc:alice123', '@alice.socialsim.dev', 'Alice Anderson', 'alice@example.com', false, 150, 75, '2025-01-01 10:00:00'),
('00000000-0000-0000-0000-000000000002', 'did:plc:bob456', '@bob.socialsim.dev', 'Bob Builder', 'bob@example.com', false, 89, 120, '2025-01-02 11:30:00'),
('00000000-0000-0000-0000-000000000003', 'did:plc:carol789', '@carol.socialsim.dev', 'Carol Chen', 'carol@example.com', false, 320, 95, '2025-01-03 09:15:00'),
('00000000-0000-0000-0000-000000000004', 'did:plc:dan101', '@dan.socialsim.dev', 'Dan Davis', 'dan@example.com', false, 45, 200, '2025-01-04 14:22:00'),
('00000000-0000-0000-0000-000000000005', 'did:plc:eve202', '@eve.socialsim.dev', 'Eve Edwards', 'eve@example.com', true, 500, 50, '2025-01-05 08:45:00'),
('00000000-0000-0000-0000-000000000006', 'did:plc:frank303', '@frank.socialsim.dev', 'Frank Foster', NULL, true, 1200, 30, '2025-01-06 16:10:00'),
('00000000-0000-0000-0000-000000000007', 'did:plc:grace404', '@grace.socialsim.dev', 'Grace Garcia', NULL, true, 80, 90, '2025-01-07 12:00:00'),
('00000000-0000-0000-0000-000000000008', 'did:plc:hank505', '@hank.socialsim.dev', 'Hank Harris', NULL, true, 220, 180, '2025-01-08 10:30:00'),
('00000000-0000-0000-0000-000000000009', 'did:plc:iris606', '@iris.socialsim.dev', 'Iris Ivanov', NULL, true, 95, 110, '2025-01-09 15:45:00'),
('00000000-0000-0000-0000-000000000010', 'did:plc:jack707', '@jack.socialsim.dev', 'Jack Johnson', 'jack@example.com', false, 175, 140, '2025-01-10 11:20:00');
```

### Sample Follows

```sql
INSERT INTO follows (follower_id, following_id, status, created_at) VALUES
('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002', 'Active', '2025-01-11 10:00:00'),
('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000003', 'Active', '2025-01-11 10:05:00'),
('00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001', 'Active', '2025-01-12 09:00:00'),
('00000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000001', 'Active', '2025-01-13 14:30:00'),
('00000000-0000-0000-0000-000000000004', '00000000-0000-0000-0000-000000000003', 'Active', '2025-01-14 11:15:00'),
('00000000-0000-0000-0000-000000000005', '00000000-0000-0000-0000-000000000006', 'Active', '2025-01-15 16:45:00');
```

### Sample Posts

```sql
INSERT INTO posts (id, author_id, content, visibility, created_at) VALUES
('10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'Hello SocialSim! First post here! 🎉', 'Public', '2025-01-11 10:30:00'),
('10000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000002', 'Building something amazing with SocialSim', 'Public', '2025-01-12 14:00:00'),
('10000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000003', 'Just discovered this platform. Excited to connect!', 'Public', '2025-01-13 09:30:00'),
('10000000-0000-0000-0000-000000000004', '00000000-0000-0000-0000-000000000001', 'Working on some data models today', 'FollowersOnly', '2025-01-14 11:00:00'),
('10000000-0000-0000-0000-000000000005', '00000000-0000-0000-0000-000000000004', 'Anyone else excited about decentralized social networks?', 'Public', '2025-01-15 16:20:00');
```

### Sample Likes

```sql
INSERT INTO likes (user_id, post_id, reaction_type, created_at) VALUES
('00000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'Like', '2025-01-11 11:00:00'),
('00000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', 'Love', '2025-01-11 12:15:00'),
('00000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', 'Like', '2025-01-12 15:00:00'),
('00000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000003', 'Like', '2025-01-13 10:00:00');
```

## Neo4j Sample Data

### Create Sample Users in Neo4j

```cypher
// Create users
CREATE (alice:User {id: '00000000-0000-0000-0000-000000000001', handle: '@alice.socialsim.dev', displayName: 'Alice Anderson', followerCount: 150, influenceScore: 0.65})
CREATE (bob:User {id: '00000000-0000-0000-0000-000000000002', handle: '@bob.socialsim.dev', displayName: 'Bob Builder', followerCount: 89, influenceScore: 0.45})
CREATE (carol:User {id: '00000000-0000-0000-0000-000000000003', handle: '@carol.socialsim.dev', displayName: 'Carol Chen', followerCount: 320, influenceScore: 0.82})
CREATE (dan:User {id: '00000000-0000-0000-0000-000000000004', handle: '@dan.socialsim.dev', displayName: 'Dan Davis', followerCount: 45, influenceScore: 0.28})
CREATE (eve:User {id: '00000000-0000-0000-0000-000000000005', handle: '@eve.socialsim.dev', displayName: 'Eve Edwards', followerCount: 500, influenceScore: 0.91});
```

### Create Sample Relationships

```cypher
// Create follows
MATCH (alice:User {id: '00000000-0000-0000-0000-000000000001'})
MATCH (bob:User {id: '00000000-0000-0000-0000-000000000002'})
CREATE (alice)-[:FOLLOWS {createdAt: datetime('2025-01-11T10:00:00Z'), strength: 0.7}]->(bob);

MATCH (bob:User {id: '00000000-0000-0000-0000-000000000002'})
MATCH (alice:User {id: '00000000-0000-0000-0000-000000000001'})
CREATE (bob)-[:FOLLOWS {createdAt: datetime('2025-01-12T09:00:00Z'), strength: 0.6}]->(alice);

MATCH (alice:User {id: '00000000-0000-0000-0000-000000000001'})
MATCH (carol:User {id: '00000000-0000-0000-0000-000000000003'})
CREATE (alice)-[:FOLLOWS {createdAt: datetime('2025-01-11T10:05:00Z'), strength: 0.8}]->(carol);
```

## Data Generation Scripts

### Generate Users (C#)

```csharp
public async Task<List<User>> GenerateUsers(int count)
{
    var faker = new Faker<User>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.DecentralizedId, f => $"did:plc:{f.Random.AlphaNumeric(10)}")
        .RuleFor(u => u.Handle, f => $"@{f.Internet.UserName()}.socialsim.dev")
        .RuleFor(u => u.DisplayName, f => f.Name.FullName())
        .RuleFor(u => u.Bio, f => f.Lorem.Sentence())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.IsSimulated, f => f.Random.Bool(0.7f))
        .RuleFor(u => u.FollowerCount, f => f.Random.Int(0, 10000))
        .RuleFor(u => u.FollowingCount, f => f.Random.Int(0, 5000))
        .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));
    
    return faker.Generate(count);
}
```

### Generate Follows (C#)

```csharp
public async Task<List<Follow>> GenerateFollows(List<User> users, double followProbability = 0.1)
{
    var follows = new List<Follow>();
    var random = new Random();
    
    foreach (var follower in users)
    {
        foreach (var following in users)
        {
            if (follower.Id == following.Id) continue;
            
            if (random.NextDouble() < followProbability)
            {
                follows.Add(new Follow
                {
                    Id = Guid.NewGuid(),
                    FollowerId = follower.Id,
                    FollowingId = following.Id,
                    Status = FollowStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 365))
                });
            }
        }
    }
    
    return follows;
}
```

### Generate Posts (C#)

```csharp
public async Task<List<Post>> GeneratePosts(List<User> users, int postsPerUser)
{
    var faker = new Faker<Post>()
        .RuleFor(p => p.Id, f => Guid.NewGuid())
        .RuleFor(p => p.Content, f => f.Lorem.Paragraph())
        .RuleFor(p => p.Visibility, f => f.PickRandom<ContentVisibility>())
        .RuleFor(p => p.CreatedAt, f => f.Date.Past(1));
    
    var posts = new List<Post>();
    
    foreach (var user in users)
    {
        var userPosts = faker.Generate(postsPerUser);
        foreach (var post in userPosts)
        {
            post.AuthorId = user.Id;
        }
        posts.AddRange(userPosts);
    }
    
    return posts;
}
```

## Loading Sample Data

### PostgreSQL

```bash
psql -U postgres -d socialsim -f sample-data.sql
```

### Neo4j

```bash
cat sample-data.cypher | cypher-shell -u neo4j -p your-password
```

### From C# Application

```csharp
public class DatabaseSeeder
{
    private readonly SocialSimDbContext _context;
    
    public async Task SeedMinimalDataset()
    {
        var users = await GenerateUsers(10);
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        
        var follows = await GenerateFollows(users, 0.3);
        await _context.Follows.AddRangeAsync(follows);
        await _context.SaveChangesAsync();
        
        var posts = await GeneratePosts(users, 5);
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();
    }
}
```

## Realistic Behavior Patterns

### Agent Behavior Templates

```json
{
  "influencer": {
    "postingFrequency": 8.5,
    "engagementRate": 0.25,
    "influenceScore": 0.92,
    "personality": {
      "extraversion": 0.95,
      "agreeableness": 0.75
    }
  },
  "lurker": {
    "postingFrequency": 0.5,
    "engagementRate": 0.05,
    "influenceScore": 0.15,
    "personality": {
      "extraversion": 0.25,
      "agreeableness": 0.60
    }
  },
  "casual_user": {
    "postingFrequency": 2.3,
    "engagementRate": 0.12,
    "influenceScore": 0.45,
    "personality": {
      "extraversion": 0.55,
      "agreeableness": 0.70
    }
  }
}
```

## Next Steps

- See [postgresql-schema.sql](./postgresql-schema.sql) for the complete database schema
- See [neo4j-model.md](./neo4j-model.md) for graph database queries
- See [migration-plan.md](./migration-plan.md) for deployment instructions
