using System.Threading.Tasks;

namespace ArmaForces.ArmaServerManager.Features.Webhooks;

public interface IWebhookClient
{
    Task Send(string content);
}