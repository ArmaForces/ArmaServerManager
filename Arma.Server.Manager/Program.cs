using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Features.Hangfire.Helpers;
using Arma.Server.Manager.Services;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    // Add Hangfire services.
                    services.AddHangfire(configuration => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseLiteDbStorage(hostContext.Configuration.GetConnectionString("HangfireConnection")));

                    // Add the processing server as IHostedService
                    services.AddHangfireServer();

                    services.AddHostedService<ModsUpdateService>();

                    services.AddSingleton<IHangfireBackgroundJobClient>(HangfireBackgroundJobClient.CreateHangfireBackgroundJobClient);
                    services.AddSingleton<IHangfireJobStorage>(HangfireJobStorage.CreateHangfireJobStorage);
                    services.AddSingleton<IHangfireManager>(HangfireManager.CreateHangfireManager);
                });
    }
}
