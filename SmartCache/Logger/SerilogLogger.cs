using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LoggingLibrary
{
    public static class SerilogLogger
    {
        public static IServiceCollection ConfigureLogging(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProperty("LocalMachineName", Environment.MachineName)
                .Enrich.WithProperty("Development", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production")
                .CreateLogger();

            return services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
        }

        public static async Task LogApiRequestResponse(HttpContext context, RequestDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var request = context.Request;
                Log.Information("[API] Request: {Method} {Path} {QueryString}", request.Method, request.Path, request.QueryString);

                await next(context);

                var response = context.Response;
                Log.Information("[API] Response: {StatusCode} in {ElapsedMilliseconds}ms", response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                LogError(ex, "[API] Unhandled API Exception");
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        public static void LogError(Exception ex, string message)
        {
            Log.Error(ex, message);
        }

        public static void LogGrainEvent(string grainName, string message)
        {
            Log.Information("[Grain] {Grain} - {Message}", grainName, message);
        }

        public static void LogSiloEvent(string eventMessage)
        {
            Log.Information("[Silo] {EventMessage}", eventMessage);
        }
    }
}
