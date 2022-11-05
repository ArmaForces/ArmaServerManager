using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class DedicatedServerFactory : IDedicatedServerFactory
    {
        private readonly IServerBuilderFactory _serverBuilderFactory;

        public DedicatedServerFactory(IServerBuilderFactory serverBuilderFactory)
        {
            _serverBuilderFactory = serverBuilderFactory;
        }

        public IDedicatedServer CreateDedicatedServer(
            int port,
            Modset modset,
            int numberOfHeadlessClients)
        {
            var builder = _serverBuilderFactory.CreateServerBuilder();
            return builder
                .OnPort(port)
                .WithModset(modset)
                .WithHeadlessClients(numberOfHeadlessClients)
                .Build();
        }

        public IDedicatedServer CreateDedicatedServer(
            int port,
            Modset modset,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess>? headlessProcesses = null)
        {
            var builder = _serverBuilderFactory.CreateServerBuilder();
            return builder
                .OnPort(port)
                .WithModset(modset)
                .WithServerProcess(armaProcess)
                .WithHeadlessClients(headlessProcesses)
                .Build();
        }
    }
}
