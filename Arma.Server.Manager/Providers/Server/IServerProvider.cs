using Arma.Server.Features.Server;
using Arma.Server.Modset;

namespace Arma.Server.Manager.Providers.Server
{
    public interface IServerProvider
    {
        IDedicatedServer GetServer(int port, IModset modset);
    }
}
