using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add databases
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("socialsimdb");

// Neo4J container for social graph
var neo4j = builder.AddContainer("neo4j", "neo4j", "latest")
    .WithEnvironment("NEO4J_AUTH", "neo4j/password123")
    .WithHttpEndpoint(port: 7474, targetPort: 7474, name: "http")
    .WithEndpoint(port: 7687, targetPort: 7687, name: "bolt");

// Redis for caching and pub/sub
var redis = builder.AddRedis("redis");

// API Service
var api = builder.AddProject<Projects.SocialSim_Api>("api")
    .WithReference(postgres)
    .WithEnvironment("ConnectionStrings__neo4j", neo4j.GetEndpoint("bolt"))
    .WithReference(redis);

// Simulation Worker
var worker = builder.AddProject<Projects.SocialSim_SimulationWorker>("worker")
    .WithReference(postgres)
    .WithEnvironment("ConnectionStrings__neo4j", neo4j.GetEndpoint("bolt"))
    .WithReference(redis);

builder.Build().Run();

