using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Servers;

namespace ArmaForces.ArmaServerManager.Features.Servers
{
    public interface IServerProvider
    {
        IDedicatedServer? GetServer(int port);

        IDedicatedServer GetServer(int port, IModset modset);
        
        List<IDedicatedServer> GetServers();
    }
}
