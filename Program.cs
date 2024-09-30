using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.Extensions.Storage.Blobs;
using Microsoft.Azure.WebJobs.Extensions.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

    })
    .ConfigureWebJobs(b =>
    {
        // Register specific storage bindings
        b.AddHttp();
        b.AddAzureStorageBlobs(); 
        b.AddAzureStorageQueues(); 
        // b.AddAzureStorageQueuesScaleForTrigger(); 
    })
    .Build();

host.Run();
