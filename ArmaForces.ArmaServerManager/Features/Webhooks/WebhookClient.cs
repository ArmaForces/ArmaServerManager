using System.Net.Http;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Common;
using ArmaForces.ArmaServerManager.Features.Webhooks.DTOs;

namespace ArmaForces.ArmaServerManager.Features.Webhooks;

internal class WebhookClient : HttpClientBase, IWebhookClient
{
    public WebhookClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task Send(string content)
    {
        await HttpPostAsync(requestUrl: null, content: new DiscordWebhook
        {
            UserName = "Arma Server",
            Content = content
        });
    }
}