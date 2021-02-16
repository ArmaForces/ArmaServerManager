using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;

namespace ArmaForces.Arma.Server.Features.Server
{
    public interface IDedicatedServerFactory
    {
        IDedicatedServer CreateDedicatedServer(
            int port,
            IModset modset,
            int numberOfHeadlessClients);

        IDedicatedServer CreateDedicatedServer(
            int port,
            IModset modset,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess> headlessProcesses);
    }
}
