using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Features.Server;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public class ServerProvider : IServerProvider
    {
        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly IParametersExtractor _parametersExtractor;
        private readonly IModsetProvider _modsetProvider;
        private readonly IServerProcessFactory _serverProcessFactory;
        private readonly IDedicatedServerFactory _dedicatedServerFactory;

        public ServerProvider(
            IParametersExtractor parametersExtractor,
            IModsetProvider modsetProvider,
            IServerProcessFactory serverProcessFactory,
            IDedicatedServerFactory dedicatedServerFactory)
        {
            _parametersExtractor = parametersExtractor;
            _modsetProvider = modsetProvider;
            _serverProcessFactory = serverProcessFactory;
            _dedicatedServerFactory = dedicatedServerFactory;
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

            _ = _servers.TryRemove(server.Port, out _);
        }

        private async Task DiscoverProcesses()
        {
            var armaServerProcesses = Process.GetProcesses()
                .Where(x => x.ProcessName.Contains("arma3server"))
                .ToList();

            if (!armaServerProcesses.Any()) return;
            
            var servers = new Dictionary<int, IServerProcess>();
            var headlessProcessesDictionary = new Dictionary<int, List<IServerProcess>>();

            foreach (var armaServerProcess in armaServerProcesses)
            {
                var result = await _parametersExtractor.ExtractParameters(armaServerProcess);
                if (result.IsSuccess)
                {
                    var parameters = result.Value;
                    var process = _serverProcessFactory.CreateServerProcess(armaServerProcess, parameters);
                    if (parameters.Client)
                    {
                        if (headlessProcessesDictionary.ContainsKey(parameters.Port))
                        {
                            headlessProcessesDictionary[parameters.Port].Add(process);
                        }
                        else
                        {
                            headlessProcessesDictionary.Add(parameters.Port, new List<IServerProcess>()
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
                    : new List<IServerProcess>();

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
            IServerProcess serverProcess,
            IEnumerable<IServerProcess> headlessClients = null)
        {
            var server = _dedicatedServerFactory.CreateDedicatedServer(
                port,
                modset,
                serverProcess,
                headlessClients);

            server.Disposed += OnServerDisposed;

            return server;
        }
    }
}
