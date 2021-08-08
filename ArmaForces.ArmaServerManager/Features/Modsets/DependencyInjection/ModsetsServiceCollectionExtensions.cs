using System;
using System.Net.Http;
using ArmaForces.Arma.Server.Config;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DependencyInjection
{
    internal static class ModsetsServiceCollectionExtensions
    {
        public static IServiceCollection AddModsetsApiClient(this IServiceCollection services)
            => services.AddHttpClientForModsetsApiClient();

        private static IServiceCollection AddHttpClientForModsetsApiClient(this IServiceCollection services)
        {
            services
                .AddHttpClient<IApiModsetClient, ApiModsetClient>()
                .ConfigureHttpClient(SetBaseAddress());

            return services;
        }

        private static Action<IServiceProvider, HttpClient> SetBaseAddress()
            => (services, client) => client.BaseAddress = new Uri(
                services.GetRequiredService<ISettings>().ApiModsetsBaseUrl ??
                throw new NotSupportedException("Modsets API is required, provide valid url address."));
    }
}
