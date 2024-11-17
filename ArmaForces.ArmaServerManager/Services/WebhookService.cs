using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.ArmaServerManager.Features.Webhooks;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Services;

public class WebhookService : IWebhookService
{
    private readonly IWebhookClient _webhookClient;
    private readonly ILogger<WebhookService> _logger;
    private readonly string? _webhookUrl;

    public WebhookService(IWebhookClient webhookClient, ISettings settings, ILogger<WebhookService> logger)
    {
        _webhookClient = webhookClient;
        _logger = logger;
        _webhookUrl = settings.WebhookUrl;
    }

    public async Task AnnounceStart()
    {
        _logger.LogInformation("Announce start");
        if (_webhookUrl == null) return;
        _logger.LogInformation("Webhook url: {WebhookUrl}", _webhookUrl);

        await Task.Delay(10000);
        await _webhookClient.Send("Manager started");
    }
}