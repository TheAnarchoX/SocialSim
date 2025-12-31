# Database Migration Plan

This document outlines the deployment strategy for SocialSim's database schema across PostgreSQL and Neo4j.

## Overview

SocialSim uses a **polyglot persistence** strategy with two databases that must be kept in sync:

1. **PostgreSQL** - Source of truth for entity data
2. **Neo4j** - Optimized graph data for network analysis

## Migration Strategy

### Phase 1: Initial Schema Creation

**PostgreSQL:**
```bash
# Create database
createdb socialsim

# Run schema
psql -U postgres -d socialsim -f postgresql-schema.sql

# Verify
psql -U postgres -d socialsim -c "\dt"
```

**Neo4j:**
```bash
# Create constraints and indexes
cat neo4j-schema.cypher | cypher-shell -u neo4j -p password

# Verify
echo "CALL db.indexes();" | cypher-shell -u neo4j -p password
```

### Phase 2: Seed Initial Data

```bash
# Load sample data
psql -U postgres -d socialsim -f sample-data.sql

# Sync to Neo4j
dotnet run --project SocialSim.Tools -- sync-to-neo4j
```

### Phase 3: Enable Change Tracking

**PostgreSQL:**
- Set up triggers for audit logging
- Enable logical replication (optional)

**Application:**
- Configure event bus (Redis)
- Start synchronization workers

## Entity Framework Core Migrations

### Setup

```bash
# Install EF tools
dotnet tool install --global dotnet-ef

# Add migration package
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Create Migration

```bash
cd src/SocialSim.Api

# Create initial migration
dotnet ef migrations add InitialCreate --context SocialSimDbContext

# Review generated migration
# Edit if needed in Migrations folder
```

### Apply Migration

```bash
# Development
dotnet ef database update --context SocialSimDbContext

# Production (via CI/CD)
dotnet ef database update --context SocialSimDbContext --connection "Server=prod-server;Database=socialsim;..."
```

### Migration Script Generation

```bash
# Generate SQL script
dotnet ef migrations script --context SocialSimDbContext --output migration.sql

# Review and apply manually in production
```

## Version Control

### Migration Naming Convention

```
YYYYMMDDHHMMSS_DescriptiveName.cs
```

Example:
```
20250101120000_InitialCreate.cs
20250115140000_AddThreadSupport.cs
20250201093000_AddCircles.cs
```

### Migration Files

Store in repository:
```
src/SocialSim.Api/
  Migrations/
    20250101120000_InitialCreate.cs
    20250101120000_InitialCreate.Designer.cs
    SocialSimDbContextModelSnapshot.cs
```

## Data Synchronization

### PostgreSQL → Neo4j Sync

**Initial Bulk Sync:**

```csharp
public class Neo4jSyncService
{
    public async Task BulkSyncUsers()
    {
        var users = await _dbContext.Users.ToListAsync();
        
        foreach (var user in users)
        {
            await _neo4jClient.Cypher
                .Merge("(u:User {id: $id})")
                .OnCreate()
                .Set("u = $props")
                .OnMatch()
                .Set("u += $props")
                .WithParams(new {
                    id = user.Id.ToString(),
                    props = new {
                        handle = user.Handle,
                        displayName = user.DisplayName,
                        followerCount = user.FollowerCount,
                        influenceScore = 0.5 // Calculate
                    }
                })
                .ExecuteWithoutResultsAsync();
        }
    }
}
```

**Event-Driven Sync:**

```csharp
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent evt)
    {
        // Write to PostgreSQL
        await _userRepo.CreateAsync(evt.User);
        
        // Sync to Neo4j
        await _neo4jSync.SyncUser(evt.User);
        
        // Invalidate caches
        await _cache.InvalidateAsync($"user:{evt.User.Id}");
    }
}
```

## Backup & Restore

### PostgreSQL Backup

```bash
# Full backup
pg_dump -U postgres socialsim > backup_$(date +%Y%m%d).sql

# Compressed backup
pg_dump -U postgres socialsim | gzip > backup_$(date +%Y%m%d).sql.gz

# Schema only
pg_dump -U postgres -s socialsim > schema_$(date +%Y%m%d).sql
```

### PostgreSQL Restore

```bash
# Restore from backup
psql -U postgres -d socialsim < backup_20250131.sql

# Restore from compressed
gunzip -c backup_20250131.sql.gz | psql -U postgres -d socialsim
```

### Neo4j Backup

```bash
# Online backup (Enterprise)
neo4j-admin backup --from=localhost:6362 --backup-dir=/backups/neo4j

# Export to Cypher
cypher-shell -u neo4j -p password --format plain "CALL apoc.export.cypher.all('/backups/export.cypher', {})"
```

### Neo4j Restore

```bash
# Stop Neo4j
neo4j stop

# Restore backup
neo4j-admin restore --from=/backups/neo4j

# Start Neo4j
neo4j start
```

## Production Deployment

### Prerequisites

1. **Database Servers:**
   - PostgreSQL 15+ instance
   - Neo4j 5+ instance
   - Redis 7+ instance

2. **Network:**
   - Application can connect to all databases
   - Firewall rules configured

3. **Credentials:**
   - Database users created
   - Connection strings configured
   - Secrets stored in Azure Key Vault / AWS Secrets Manager

### Deployment Steps

**1. Database Provisioning:**

```bash
# PostgreSQL (Azure)
az postgres flexible-server create \
  --name socialsim-db \
  --resource-group socialsim-rg \
  --location eastus \
  --admin-user dbadmin \
  --admin-password <password> \
  --sku-name Standard_D4s_v3 \
  --version 15

# Neo4j (Aura)
# Provision via Neo4j Aura console
```

**2. Run Migrations:**

```bash
# Set connection string
export ConnectionStrings__SocialSimDb="Host=socialsim-db.postgres.database.azure.com;..."

# Apply migrations
dotnet ef database update --project src/SocialSim.Api
```

**3. Verify Schema:**

```bash
# PostgreSQL
psql -h socialsim-db.postgres.database.azure.com -U dbadmin -d socialsim -c "\dt"

# Neo4j
echo "CALL db.schema.visualization();" | cypher-shell -a neo4j+s://xxx.databases.neo4j.io -u neo4j -p <password>
```

**4. Sync Initial Data:**

```bash
dotnet run --project src/SocialSim.Tools -- sync-all
```

## Rollback Plan

### PostgreSQL Rollback

```bash
# Revert to previous migration
dotnet ef database update PreviousMigrationName --context SocialSimDbContext

# Or restore from backup
psql -U postgres -d socialsim < backup_before_migration.sql
```

### Neo4j Rollback

```bash
# No built-in migration versioning
# Restore from backup or manually revert changes
neo4j-admin restore --from=/backups/before_migration
```

## Monitoring

### Health Checks

```csharp
services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddNeo4j(neo4jConnectionString, name: "neo4j")
    .AddRedis(redisConnectionString, name: "redis");
```

### Migration Tracking

```sql
-- PostgreSQL EF Migrations History
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC;
```

### Sync Status

```sql
-- Create sync status table
CREATE TABLE sync_status (
    entity_type VARCHAR(64) PRIMARY KEY,
    last_sync_at TIMESTAMP NOT NULL,
    record_count BIGINT NOT NULL,
    sync_status VARCHAR(32) NOT NULL
);
```

## Troubleshooting

### Common Issues

**Issue: Migration fails with FK constraint error**
```bash
# Solution: Check migration order, ensure parent tables exist first
dotnet ef migrations remove
# Fix migration file
dotnet ef migrations add FixedMigration
```

**Issue: Neo4j sync lag**
```bash
# Solution: Check Redis event queue
redis-cli LLEN events:neo4j-sync
# Process backlog manually if needed
```

**Issue: Connection pool exhaustion**
```csharp
// Solution: Tune connection pool settings
services.AddDbContext<SocialSimDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MaxBatchSize(100)
              .CommandTimeout(30)));
```

## Next Steps

After successful migration:

1. **Load test data**: See [sample-data.md](./sample-data.md)
2. **Run tests**: Verify schema with integration tests
3. **Monitor performance**: Check query performance
4. **Document**: Update runbooks and operational docs
