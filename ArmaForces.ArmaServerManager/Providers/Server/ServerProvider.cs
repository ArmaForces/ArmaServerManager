using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Server;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public class ServerProvider : IServerProvider
    {
        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly IModsetProvider _modsetProvider;
        private readonly IArmaProcessDiscoverer _armaProcessDiscoverer;
        private readonly IDedicatedServerFactory _dedicatedServerFactory;
        private readonly ILogger<ServerProvider> _logger;

        public ServerProvider(
            IModsetProvider modsetProvider,
            IArmaProcessDiscoverer armaProcessDiscoverer,
            IDedicatedServerFactory dedicatedServerFactory,
            ILogger<ServerProvider> logger)
        {
            _modsetProvider = modsetProvider;
            _armaProcessDiscoverer = armaProcessDiscoverer;
            _dedicatedServerFactory = dedicatedServerFactory;
            _logger = logger;
            DiscoverProcesses();
        }

        public IDedicatedServer GetServer(int port)
        {
            return _servers.TryGetValue(port, out var server)
                ? server
                : null;
        }

        public IDedicatedServer GetServer(int port, IModset modset)
            => _servers.GetOrAdd(port, serverPort => CreateServer(serverPort, modset, 1));
        
        private async Task OnServerDisposed(IDedicatedServer dedicatedServer)
        {
            _logger.LogInformation("Server disposed, removing.");

            _ = _servers.TryRemove(dedicatedServer.Port, out _);
        }

        private async Task DiscoverProcesses()
        {
            var armaProcesses = await _armaProcessDiscoverer.DiscoverArmaProcesses();
            
            foreach (var serverKeyValuePair in armaProcesses)
            {
                var (port, processes) = serverKeyValuePair;
                var server = processes.SingleOrDefault(x => x.ProcessType == ArmaProcessType.Server);
                var headlessClients = processes.Where(x => x.ProcessType == ArmaProcessType.HeadlessClient);

                var dedicatedServer = CreateServer(
                    port,
                    _modsetProvider.GetModsetByName(server?.Parameters.ModsetName).Value,
                    server,
                    headlessClients);

                _servers.GetOrAdd(port, dedicatedServer);
            }
        }

        private IDedicatedServer CreateServer(int port, IModset modset, int numberOfHeadlessClients)
        {
            var server = _dedicatedServerFactory.CreateDedicatedServer(port, modset, numberOfHeadlessClients);

            server.OnServerShutdown += OnServerDisposed;

            return server;
        }

        private IDedicatedServer CreateServer(
            int port,
            IModset modset,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess> headlessClients = null)
        {
            var server = _dedicatedServerFactory.CreateDedicatedServer(
                port,
                modset,
                armaProcess,
                headlessClients);

            return server;
        }
    }
}
