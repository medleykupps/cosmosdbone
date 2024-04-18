using CosmosDbOneApp;
using CosmosDbOneApp.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new HostBuilder();

builder.ConfigureServices(services =>
{
    services.AddTransient<ICyclistRepository, CyclistRepository>();
    services.AddSingleton(x => new CosmosDbClientFactory().CreateClient());
});

var host = builder
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();

