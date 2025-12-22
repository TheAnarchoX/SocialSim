using Microsoft.EntityFrameworkCore;
using SocialSim.Api.Data;
using SocialSim.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire service discovery
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddOpenApi();

// Database configuration
builder.AddNpgsqlDbContext<SocialSimDbContext>("socialsimdb");

// Neo4J configuration
builder.Services.AddSingleton<Neo4j.Driver.IDriver>(sp =>
{
    var neo4jUri = builder.Configuration.GetConnectionString("neo4j") ?? "bolt://localhost:7687";
    var neo4jUsername = builder.Configuration["Neo4j:Username"] ?? "neo4j";
    var neo4jPassword = builder.Configuration["Neo4j:Password"] ?? "password123";
    return Neo4j.Driver.GraphDatabase.Driver(neo4jUri, Neo4j.Driver.AuthTokens.Basic(neo4jUsername, neo4jPassword));
});

// Configure AT Protocol options
builder.Services.Configure<SocialSim.Core.Configuration.ATProtocolOptions>(
    builder.Configuration.GetSection("ATProtocol"));

// SignalR for real-time updates
builder.Services.AddSignalR();

// CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseHttpsRedirection();

// Map SignalR hub
app.MapHub<SimulationHub>("/simulationHub");

// Sample endpoint
app.MapGet("/api/status", () => new 
{ 
    Status = "Running", 
    Timestamp = DateTime.UtcNow,
    Service = "SocialSim API"
})
.WithName("GetStatus");

app.Run();

