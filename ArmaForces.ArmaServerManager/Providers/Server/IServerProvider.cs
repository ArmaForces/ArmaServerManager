using ArmaForces.Arma.Server.Features.Server;
using ArmaForces.Arma.Server.Modset;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public interface IServerProvider
    {
        IDedicatedServer GetServer(int port);

        IDedicatedServer GetServer(int port, IModset modset);
    }
}
