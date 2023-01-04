using System.Collections.Generic;
using System.Diagnostics;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public interface IArmaProcessFactory
    {
        IArmaProcess CreateServerProcess(Process process, ProcessParameters parameters);

        IArmaProcess CreateServerProcess(
            int port,
            Modset modset,
            IModsetConfig modsetConfig);

        IEnumerable<IArmaProcess> CreateHeadlessClients(
            int port,
            Modset modset,
            IModsetConfig modsetConfig,
            int numberOfHeadlessClients = 1);
    }
}
