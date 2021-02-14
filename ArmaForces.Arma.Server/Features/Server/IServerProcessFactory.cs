using System.Collections.Generic;
using System.Diagnostics;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;

namespace ArmaForces.Arma.Server.Features.Server
{
    public interface IServerProcessFactory
    {
        IServerProcess CreateServerProcess(Process process, ServerParameters parameters = null);

        IServerProcess CreateServerProcess(
            int port,
            IModset modset,
            IModsetConfig modsetConfig);

        IEnumerable<IServerProcess> CreateHeadlessClients(
            int port,
            IModset modset,
            IModsetConfig modsetConfig,
            int numberOfHeadlessClients = 1);
    }
}
