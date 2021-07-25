using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
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

        public IDedicatedServer? GetServer(int port)
        {
            return _servers.TryGetValue(port, out var server)
                ? server
                : null;
        }

        public IDedicatedServer GetServer(int port, IModset modset)
        {
            var server = _servers.GetOrAdd(
                port,
                serverPort => CreateServer(
                    serverPort,
                    modset,
                    1));

            return server.Modset.Equals(modset)
                ? server
                // TODO: Think of better exception type here
                : throw new ServerMismatchException(
                    $"Expected to get server with {modset.Name} modset with {modset.Mods.Count} mods on port {port} but found {server.Modset.Name} with {server.Modset.Mods.Count} mods.");
        }

        private async Task OnServerDisposed(IDedicatedServer dedicatedServer)
        {
            _logger.LogDebug(
                "Server with {ModsetName} disposed on port {Port}, removing",
                dedicatedServer.Modset.Name,
                dedicatedServer.Port);

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

                if (server is null)
                {
                    _logger.LogDebug("Server not found on port {Port}. Shutting down headless clients for server on this port", port);

                    foreach (var headlessClient in headlessClients)
                    {
                        headlessClient.Shutdown();
                    }

                    _logger.LogDebug("Headless clients for server on port {Port} were shut down", port);

                    continue;
                }

                var dedicatedServer = CreateServer(
                    port,
                    // TODO: Handle the result of GetModsetByName
                    _modsetProvider.GetModsetByName(server.Parameters.ModsetName).Value,
                    server,
                    headlessClients);

                dedicatedServer.OnServerShutdown += OnServerDisposed;

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
            IEnumerable<IArmaProcess>? headlessClients = null)
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
