using System.Threading.Tasks;

namespace ArmaForces.ArmaServerManager.Services;

public interface IWebhookService
{
    Task AnnounceStart();
}