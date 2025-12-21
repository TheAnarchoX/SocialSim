using SocialSim.SimulationWorker;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add the simulation worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
