using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

namespace SmartCacheSilo;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseOrleans(siloBuilder =>
            {
                siloBuilder
                    .UseLocalhostClustering()
                    .UseInMemoryReminderService()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev-cluster";
                        options.ServiceId = "EmailDomainService";
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddAzureBlobGrainStorage("AzureBlobStorage", options =>
                        {
                            options.BlobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true;");
                        });
                    })
                    .ConfigureLogging(logging => logging.AddConsole());
            });
}