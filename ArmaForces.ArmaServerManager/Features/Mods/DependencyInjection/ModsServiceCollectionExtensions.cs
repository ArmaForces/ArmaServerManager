using ArmaForces.ArmaServerManager.Features.Modsets.DependencyInjection;
using ArmaForces.ArmaServerManager.Features.Steam;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.ArmaServerManager.Features.Mods.DependencyInjection
{
    internal static class ModsServiceCollectionExtensions
    {
        public static IServiceCollection AddMods(this IServiceCollection services)
        {
            return services
                .AddSingleton<ModsCache>()
                .AddSingleton<IModsCache, ModsCache>()
                .AddSingleton<IModsManager, ModsManager>()
                .AddContent()
                .AddSingleton<IWebModsetMapper, ModsCache>()
                .AddModsetsApiClient()
                .AddSingleton<IModsetProvider, ModsetProvider>();
        }

        private static IServiceCollection AddContent(this IServiceCollection services)
            => services
                .AddSingleton<ISteamClient, SteamClient>()
                .AddSingleton<IManifestDownloader, ManifestDownloader>()
                .AddSingleton<IContentDownloader, ContentDownloader>()
                .AddSingleton<IContentVerifier, ContentVerifier>()
                .AddSingleton<IContentFileVerifier, ContentFileVerifier>();
    }
}
