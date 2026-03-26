using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Modsets.DependencyInjection;
using ArmaForces.ArmaServerManager.Features.Steam;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Features.Steam.RemoteStorage;
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
                .AddScoped<IModsManager, ModsManager>()
                .AddContent()
                .AddSingleton<IWebModsetMapper, ModsCache>()
                .AddModsetsApiClient()
                .AddSingleton<IModsetProvider, ModsetProvider>();
        }

        private static IServiceCollection AddContent(this IServiceCollection services)
            => services
                .AddScoped<ISteamClient, SteamClient>()
                .AddScoped<ISteamRemoteStorage, SteamRemoteStorage>()
                .AddScoped<IManifestDownloader, ManifestDownloader>()
                .AddScoped<IContentDownloader, ContentDownloader>()
                .AddScoped<IContentVerifier, ContentVerifier>()
                .AddScoped<IContentFileVerifier, ContentFileVerifier>();
    }
}
