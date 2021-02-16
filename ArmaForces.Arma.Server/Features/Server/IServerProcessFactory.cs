﻿using System.Collections.Generic;
using System.Diagnostics;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;

namespace ArmaForces.Arma.Server.Features.Server
{
    public interface IServerProcessFactory
    {
        IArmaProcess CreateServerProcess(Process process, ServerParameters parameters = null);

        IArmaProcess CreateServerProcess(
            int port,
            IModset modset,
            IModsetConfig modsetConfig);

        IEnumerable<IArmaProcess> CreateHeadlessClients(
            int port,
            IModset modset,
            IModsetConfig modsetConfig,
            int numberOfHeadlessClients = 1);
    }
}
