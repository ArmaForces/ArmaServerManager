using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public interface IDedicatedServerFactory
    {
        IDedicatedServer CreateDedicatedServer(
            int port,
            Modset modset,
            int numberOfHeadlessClients);

        IDedicatedServer CreateDedicatedServer(
            int port,
            Modset modset,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess>? headlessProcesses = null);
    }
}
