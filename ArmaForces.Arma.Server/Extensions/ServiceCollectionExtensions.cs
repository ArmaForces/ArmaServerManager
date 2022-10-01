using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Configuration.Providers;
using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Keys.Finder;
using ArmaForces.Arma.Server.Features.Keys.IO;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Features.Parameters.Extractors;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.Arma.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArmaServer(this IServiceCollection services)
        {
            return services
                .AddConfig()
                .AddKeys()
                .AddMods()
                .AddProcess()
                .AddServer();
        }

        private static IServiceCollection AddConfig(this IServiceCollection services)
        {
            return services
                .AddSingleton<ISettings>(Settings.LoadSettings)
                .AddSingleton<IConfig, ServerConfig>()
                // TODO: Change to scoped after refactoring ServerProvider
                .AddSingleton<ConfigFileCreator>()
                .AddSingleton<ConfigReplacer>()
                .AddSingleton<IModsetConfigurationProvider, ModsetConfigurationProvider>();
        }

        private static IServiceCollection AddKeys(this IServiceCollection services)
        {
            return services
                    // TODO: Change to scoped after refactoring ServerProvider
                .AddSingleton<IKeysPreparer, KeysPreparer>()
                .AddSingleton<IKeysFinder, KeysFinder>()
                .AddSingleton<IKeysCopier, KeysCopier>();
        }

        private static IServiceCollection AddMods(this IServiceCollection services)
        {
            return services
                    // TODO: Change to scoped after refactoring ModsCache
                .AddSingleton<IModDirectoryFinder, ModDirectoryFinder>();
        }

        private static IServiceCollection AddProcess(this IServiceCollection services)
        {
            return services
                    // TODO: Change to scoped after refactoring ServerProvider
                .AddSingleton<IArmaProcessDiscoverer, ArmaProcessDiscoverer>()
                .AddSingleton<IArmaProcessFactory, ArmaProcessFactory>()
                .AddSingleton<IArmaProcessManager, ArmaProcessManager>();
        }

        private static IServiceCollection AddServer(this IServiceCollection services)
        {
            return services
                    // TODO: Change to scoped after refactoring ServerProvider
                .AddSingleton<IServerBuilder, ServerBuilder>()
                .AddSingleton<IServerBuilderFactory, ServerBuilderFactory>()
                .AddSingleton<IServerStatusFactory, ServerStatusFactory>()
                .AddSingleton<IDedicatedServerFactory, DedicatedServerFactory>()
                .AddSingleton<IParametersExtractor, ParametersExtractor>();
        }
    }
}
