using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Features.Server;
using ArmaForces.Arma.Server.Providers.Configuration;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public class ServerProvider : IServerProvider
    {
        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly IServiceProvider _serviceProvider;
        private readonly ParametersExtractor _parametersExtractor;
        private readonly IModsetProvider _modsetProvider;

        public ServerProvider(
            IServiceProvider serviceProvider,
            ParametersExtractor parametersExtractor,
            IModsetProvider modsetProvider)
        {
            _serviceProvider = serviceProvider;
            _parametersExtractor = parametersExtractor;
            _modsetProvider = modsetProvider;
            DiscoverProcesses();
        }

        public IDedicatedServer GetServer(int port)
        {
            return _servers.TryGetValue(port, out var server)
                ? server
                : null;
        }

        public IDedicatedServer GetServer(int port, IModset modset)
            => _servers.GetOrAdd(port, CreateServer(port, modset));
        
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
            
            foreach (var armaServerProcess in armaServerProcesses)
            {
                ServerParameters parameters = null;
                await _parametersExtractor.ExtractParameters(armaServerProcess)
                    .Tap(x => parameters = x)
                    // TODO: Handle assigning HC to server
                    .Bind(x => _modsetProvider.GetModsetByName(x.ModsetName))
                    .Bind(modset => Result.Success(CreateServer(parameters.Port, modset, armaServerProcess)))
                    .Tap(server => _servers.GetOrAdd(parameters.Port, server));
            }
        }

        private IDedicatedServer CreateServer(int port, IModset modset)
        {
            var server = new DedicatedServer(
                port,
                _serviceProvider.GetService<ISettings>(),
                modset,
                _serviceProvider.GetService<IServerConfigurationProvider>(),
                _serviceProvider.GetService<ILogger<DedicatedServer>>(),
                _serviceProvider.GetService<ILogger<ServerProcess>>());

            server.Disposed += OnServerDisposed;

            return server;
        }

        private IDedicatedServer CreateServer(int port, IModset modset, Process process)
        {
            var serverProcess = new ServerProcess(process, _serviceProvider.GetService<ILogger<ServerProcess>>());

            var server = new DedicatedServer(
                port,
                _serviceProvider.GetService<ISettings>(),
                modset,
                _serviceProvider.GetService<IServerConfigurationProvider>(),
                _serviceProvider.GetService<ILogger<DedicatedServer>>(),
                _serviceProvider.GetService<ILogger<ServerProcess>>(),
                serverProcess);

            server.Disposed += OnServerDisposed;

            return server;
        }
    }
}
