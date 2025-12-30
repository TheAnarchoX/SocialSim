using SocialSim.SimulationWorker;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Configure AT Protocol options
builder.Services.Configure<SocialSim.Core.Configuration.ATProtocolOptions>(
    builder.Configuration.GetSection("ATProtocol"));

// Add the simulation worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
