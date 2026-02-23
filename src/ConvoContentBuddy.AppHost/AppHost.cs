using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for SignalR backplane
var redis = builder.AddRedis("redis");

// Add PostgreSQL for relational data
var postgres = builder.AddPostgres("postgres");
var db = postgres.AddDatabase("convocontentbuddy");

// Add Qdrant for vector search
var qdrant = builder.AddQdrant("qdrant");

// Add API.Brain with TMR (3 replicas)
var api = builder.AddProject<ConvoContentBuddy_API_Brain>("api-brain")
    .WithReference(redis)
    .WithReference(db)
    .WithReference(qdrant)
    .WithReplicas(3);

// Add Blazor WASM UI
builder.AddProject<ConvoContentBuddy_UI_Web>("ui-web")
    .WithReference(api);

// Add Data Seeder worker
builder.AddProject<ConvoContentBuddy_Data_Seeder>("data-seeder")
    .WithReference(db)
    .WithReference(qdrant);

builder.Build().Run();
