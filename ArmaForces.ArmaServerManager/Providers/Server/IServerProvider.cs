﻿using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Server;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public interface IServerProvider
    {
        IDedicatedServer GetServer(int port);

        IDedicatedServer GetServer(int port, IModset modset);
    }
}
