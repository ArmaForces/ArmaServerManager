using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Keys.Finder;
using ArmaForces.Arma.Server.Features.Keys.IO;
using ArmaForces.Arma.Server.Features.Mods;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.Arma.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArmaServer(this IServiceCollection services)
        {
            return services
                .AddKeys()
                .AddMods();
        }

        private static IServiceCollection AddKeys(this IServiceCollection services)
        {
            return services
                .AddScoped<IKeysPreparer, KeysPreparer>()
                .AddScoped<IKeysFinder, KeysFinder>()
                .AddScoped<IKeysCopier, KeysCopier>();
        }

        private static IServiceCollection AddMods(this IServiceCollection services)
        {
            return services
                .AddScoped<IModDirectoryFinder, ModDirectoryFinder>();
        }
    }
}
