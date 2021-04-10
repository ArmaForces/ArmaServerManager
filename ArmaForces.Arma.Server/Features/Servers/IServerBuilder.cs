using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public interface IServerBuilder
    {
        IServerBuilder OnPort(int port);

        IServerBuilder WithModset(IModset modset);

        IServerBuilder WithServerProcess(IArmaProcess armaProcess);

        IServerBuilder WithHeadlessClients(IEnumerable<IArmaProcess>? headlessClients = null);

        IServerBuilder WithHeadlessClients(int numberOfHeadlessClients);
        
        IDedicatedServer Build();
    }
}
