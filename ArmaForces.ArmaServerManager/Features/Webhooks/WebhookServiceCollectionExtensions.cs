using System;
using System.Net.Http;
using ArmaForces.Arma.Server.Config;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaForces.ArmaServerManager.Features.Webhooks;

internal static class WebhookServiceCollectionExtensions
{
    public static IServiceCollection AddWebhookClient(this IServiceCollection services)
        => services.AddHttpClientForWebhookClient();
    
    private static IServiceCollection AddHttpClientForWebhookClient(this IServiceCollection services)
    {
        services
            .AddHttpClient<IWebhookClient, WebhookClient>()
            .ConfigureHttpClient(SetBaseAddress());

        return services;
    }

    private static Action<IServiceProvider, HttpClient> SetBaseAddress()
        => (services, client) => client.BaseAddress = new Uri(
            services.GetRequiredService<ISettings>().WebhookUrl ??
            throw new NotSupportedException("Missions API is required, provide valid url address."));
}