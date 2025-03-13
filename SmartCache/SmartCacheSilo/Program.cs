using LoggingLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Serilog;

namespace SmartCacheSilo;

class Program
{
    static async Task Main(string[] args)
    {
        SerilogLogger.ConfigureLogging(new ServiceCollection());

        try
        {
            SerilogLogger.LogSiloEvent("Orleans Silo is starting");
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
            SerilogLogger.LogSiloEvent("Orleans Silo is running.");
        }
        catch (Exception ex)
        {
            SerilogLogger.LogError(ex, "Orleans Silo encountered an error!");
        }
        finally
        {
            SerilogLogger.LogSiloEvent("Orleans Silo is shutting down.");
        }
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
                        SerilogLogger.ConfigureLogging(services);
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog();
                    });
            });
}