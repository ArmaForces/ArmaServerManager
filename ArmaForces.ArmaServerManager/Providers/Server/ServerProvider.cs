using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Features.Server;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public class ServerProvider : IServerProvider
    {
        private const string ArmaProcessName = "arma3server";

        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly IParametersExtractor _parametersExtractor;
        private readonly IModsetProvider _modsetProvider;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly IDedicatedServerFactory _dedicatedServerFactory;
        private readonly ILogger<ServerProvider> _logger;

        public ServerProvider(
            IParametersExtractor parametersExtractor,
            IModsetProvider modsetProvider,
            IArmaProcessFactory armaProcessFactory,
            IDedicatedServerFactory dedicatedServerFactory,
            ILogger<ServerProvider> logger)
        {
            _parametersExtractor = parametersExtractor;
            _modsetProvider = modsetProvider;
            _armaProcessFactory = armaProcessFactory;
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
            => _servers.GetOrAdd(port, CreateServer(port, modset, 1));
        
        private void OnServerDisposed(object sender, EventArgs e)
        {
            if (!(sender is IDedicatedServer server)) return;

            _logger.LogInformation("Server disposed, removing.");

            _ = _servers.TryRemove(server.Port, out _);
        }

        private async Task DiscoverProcesses()
        {
            var armaServerProcesses = Process.GetProcesses()
                .Where(x => x.ProcessName.Contains(ArmaProcessName))
                .ToList();

            if (!armaServerProcesses.Any()) return;
            
            var servers = new Dictionary<int, IArmaProcess>();
            var headlessProcessesDictionary = new Dictionary<int, List<IArmaProcess>>();

            _logger.LogInformation($"Found {{count}} running {ArmaProcessName} processes.", armaServerProcesses.Count);
            foreach (var armaServerProcess in armaServerProcesses)
            {
                var result = await _parametersExtractor.ExtractParameters(armaServerProcess);
                if (result.IsSuccess)
                {
                    var parameters = result.Value;
                    var process = _armaProcessFactory.CreateServerProcess(armaServerProcess, parameters);
                    if (parameters.Client)
                    {
                        if (headlessProcessesDictionary.ContainsKey(parameters.Port))
                        {
                            headlessProcessesDictionary[parameters.Port].Add(process);
                        }
                        else
                        {
                            headlessProcessesDictionary.Add(parameters.Port, new List<IArmaProcess>()
                            {
                                process
                            });
                        }
                    }
                    else
                    {
                        if (servers.ContainsKey(parameters.Port))
                        {
                            throw new NotImplementedException("There are 2 servers running on the same port.");
                        }

                        servers.Add(parameters.Port, process);
                    }
                }
                else
                {
                    throw new NotImplementedException("Parameters extraction from process failed.");
                }
            }

            foreach (var serverKeyValuePair in servers)
            {
                var (port, server) = serverKeyValuePair;
                var headlessClients = headlessProcessesDictionary.ContainsKey(port)
                    ? headlessProcessesDictionary[port]
                    : new List<IArmaProcess>();

                var dedicatedServer = CreateServer(
                    port,
                    _modsetProvider.GetModsetByName(server.Parameters.ModsetName).Value,
                    server,
                    headlessClients);

                _servers.GetOrAdd(port, dedicatedServer);
            }
        }

        private IDedicatedServer CreateServer(int port, IModset modset, int numberOfHeadlessClients)
        {
            var server = _dedicatedServerFactory.CreateDedicatedServer(port, modset, numberOfHeadlessClients);

            server.Disposed += OnServerDisposed;

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
