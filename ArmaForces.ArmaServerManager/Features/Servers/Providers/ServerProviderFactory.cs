using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.ArmaServerManager.Features.Modsets;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Servers.Providers
{
    internal class ServerProviderFactory
    {
        private readonly IModsetProvider _modsetProvider;
        private readonly IArmaProcessDiscoverer _armaProcessDiscoverer;
        private readonly IDedicatedServerFactory _dedicatedServerFactory;
        private readonly ILogger<ServerProviderFactory> _logger;

        public ServerProviderFactory(
            IModsetProvider modsetProvider,
            IArmaProcessDiscoverer armaProcessDiscoverer,
            IDedicatedServerFactory dedicatedServerFactory,
            ILogger<ServerProviderFactory> logger)
        {
            _modsetProvider = modsetProvider;
            _armaProcessDiscoverer = armaProcessDiscoverer;
            _dedicatedServerFactory = dedicatedServerFactory;
            _logger = logger;
        }
        
        public async Task<IServerProvider> CreateServerProviderAsync(IServiceProvider serviceProvider)
        {
            var servers = await LoadRunningServers();

            if (servers.Any())
            {
                _logger.LogInformation("Loaded {Count} running servers", servers.Count);
            }
            else
            {
                _logger.LogInformation("No servers recreated");
            }

            return new ServerProvider(
                servers,
                serviceProvider.GetRequiredService<ILogger<ServerProvider>>());
        }

        private async Task<Dictionary<int, IDedicatedServer>> LoadRunningServers()
        {
            _logger.LogDebug("Loading running servers");
            
            var potentialServers = await _armaProcessDiscoverer.DiscoverArmaProcesses();

            if (potentialServers.IsEmpty())
            {
                _logger.LogDebug("Found no running servers");
                
                return new Dictionary<int, IDedicatedServer>();
            }
            
            _logger.LogDebug("Found {Count} potential arma servers", potentialServers.Count);

            var dictionary = new Dictionary<int, IDedicatedServer>();
        
            foreach (var (port, processes) in potentialServers)
            {
                var serverProcess = processes
                    .SingleOrDefault(x => x.ProcessType == ArmaProcessType.Server);
                
                var headlessClients = processes
                    .Where(x => x.ProcessType == ArmaProcessType.HeadlessClient)
                    .ToList();

                if (serverProcess is null)
                {
                    _logger.LogDebug("Server not found on port {Port}. Shutting down {Count} headless clients for server on this port", port, headlessClients.Count);

                    ShutdownHeadlessClients(port, headlessClients);
                }
                else
                {
                    _logger.LogDebug("Trying to load server on port {Port} with {Count} HCs", port, headlessClients.Count);
                    
                    await TryRecreateServer(port, serverProcess, headlessClients)
                        .Tap(server => dictionary.Add(port, server))
                        .Tap(server => _logger.LogInformation("Successfully loaded server on port {Port} with {Modset} modset", server.Port, server.Modset.Name));
                }
            }
        
            return dictionary;
        }

        private void ShutdownHeadlessClients(int port, IEnumerable<IArmaProcess> headlessClients)
        {
            foreach (var headlessClient in headlessClients)
            {
                headlessClient.Shutdown();
            }

            _logger.LogDebug("Headless clients for server on port {Port} were shut down", port);
        }

        private async Task<Result<IDedicatedServer>> TryRecreateServer(
            int port,
            IArmaProcess serverProcess,
            IEnumerable<IArmaProcess> headlessClients)
            => await _modsetProvider.GetModsetByName(serverProcess.Parameters.ModsetName)
                .Map(modset => RecreateRunningServer(port, modset, serverProcess, headlessClients))
                .OnFailure(error => _logger.LogError("Could not load server on port {Port} due to error {Error}", port, error));

        private IDedicatedServer RecreateRunningServer(
            int port,
            IModset modset,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess>? headlessClients = null)
            => _dedicatedServerFactory.CreateDedicatedServer(
                port,
                modset,
                armaProcess,
                headlessClients);
    }
}