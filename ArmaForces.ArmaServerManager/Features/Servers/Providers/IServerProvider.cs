using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Servers;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Servers.Providers
{
    public interface IServerProvider
    {
        IDedicatedServer? GetServer(int port);

        IDedicatedServer GetOrAddServer(int port, Func<int, IDedicatedServer> serverFactory);
        
        List<IDedicatedServer> GetServers();

        Result TryRemoveServer(IDedicatedServer dedicatedServer);
    }
}
