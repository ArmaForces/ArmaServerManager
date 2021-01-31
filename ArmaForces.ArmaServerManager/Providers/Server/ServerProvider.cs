using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Server;
using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Providers.Server
{
    public class ServerProvider : IServerProvider
    {
        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly IServiceProvider _serviceProvider;

        public ServerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            const string portString = "port ";

            var armaServerProcesses = Process.GetProcesses()
                .Where(x => x.ProcessName.Contains("arma3server"))
                .ToList();

            if (!armaServerProcesses.Any()) return;

            var hangfireManager = _serviceProvider.GetService<IHangfireManager>();

            foreach (var armaServerProcess in armaServerProcesses)
            {
                var portStringIndex = armaServerProcess.MainWindowTitle.LastIndexOf(portString, StringComparison.InvariantCulture);
                var portIntString = armaServerProcess.MainWindowTitle.Substring(portStringIndex + portString.Length);
                var parseSuccess = int.TryParse(portIntString, out var port);

                if (!parseSuccess) continue;

                var unknownModset = new Modset
                {
                    Name = "Unknown"
                };

                var server = CreateServer(port, unknownModset, armaServerProcess);
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var serverStatus = await server.GetServerStatusAsync(cancellationTokenSource.Token);
                if (serverStatus.Players == 0)
                {
                    server.Shutdown();
                }
                else
                {
                    _servers.GetOrAdd(port, server);
                    hangfireManager.ScheduleJob<ServerStartupService>(x => x.ShutdownServer(port, false, CancellationToken.None));
                }
            }
        }

        private IDedicatedServer CreateServer(int port, IModset modset)
        {
            var server = new DedicatedServer(
                port,
                _serviceProvider.GetService<ISettings>(),
                modset,
                _serviceProvider.GetService<IKeysProvider>(),
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
                _serviceProvider.GetService<IKeysProvider>(),
                _serviceProvider.GetService<IServerConfigurationProvider>(),
                _serviceProvider.GetService<ILogger<DedicatedServer>>(),
                _serviceProvider.GetService<ILogger<ServerProcess>>(),
                serverProcess);

            server.Disposed += OnServerDisposed;

            return server;
        }
    }
}
