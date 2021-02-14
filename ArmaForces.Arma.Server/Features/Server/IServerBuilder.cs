using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;

namespace ArmaForces.Arma.Server.Features.Server
{
    public interface IServerBuilder
    {
        IServerBuilder OnPort(int port);

        IServerBuilder WithModset(IModset modset);

        IServerBuilder WithServerProcess(IServerProcess serverProcess);

        IServerBuilder WithHeadlessClients(IEnumerable<IServerProcess> headlessClients);

        IServerBuilder WithHeadlessClients(int numberOfHeadlessClients);
        
        IDedicatedServer Build();
    }
}
