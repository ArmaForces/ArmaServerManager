using System;
using System.IO;
using ArmaForces.Arma.Server.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ArmaForces.ArmaServerManager.Infrastructure.Logging
{
    internal static class SerilogConfigurationExtensions
    {
        private const string LogsDirectoryName = "Logs";
        private const string LogFileName = "Log.txt";
        private const string OutputTemplate = "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
            => hostBuilder.UseSerilog(ConfigureLogging());

        private static Action<HostBuilderContext, IServiceProvider, LoggerConfiguration> ConfigureLogging() => (context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .WriteTo.File(GetFileLogsPath(services),
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                outputTemplate: OutputTemplate);

        private static string GetFileLogsPath(IServiceProvider services)
        {
            var settings = services.GetRequiredService<ISettings>();
            
            return Path.Join(settings.ManagerDirectory, LogsDirectoryName, LogFileName);
        }
    }
}
