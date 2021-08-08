using System;
using System.Net.Http;
using ArmaForces.Arma.Server.Config;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.ArmaServerManager.Features.Missions.DependencyInjection
{
    internal static class MissionsServiceCollectionExtensions
    {
        public static IServiceCollection AddMissionsApiClient(this IServiceCollection services)
            => services
                .AddSingleton<IApiMissionsClient, ApiMissionsClient>()
                .AddHttpClientForMissionsApiClient();

        private static IServiceCollection AddHttpClientForMissionsApiClient(this IServiceCollection services)
        {
            services
                .AddHttpClient<IApiMissionsClient, ApiMissionsClient>()
                .ConfigureHttpClient(SetBaseAddress());

            return services;
        }

        private static Action<IServiceProvider, HttpClient> SetBaseAddress()
            => (services, client) => client.BaseAddress = new Uri(
                services.GetRequiredService<ISettings>().ApiMissionsBaseUrl ??
                throw new NotSupportedException("Missions API is required, provide valid url address."));
    }
}
