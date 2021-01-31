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
        private const string ArmaProcessName = "arma3server";

        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServerProvider> _logger;

        public ServerProvider(IServiceProvider serviceProvider, ILogger<ServerProvider> logger)
        {
            _serviceProvider = serviceProvider;
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
            => _servers.GetOrAdd(port, CreateServer(port, modset));
        
        private void OnServerDisposed(object sender, EventArgs e)
        {
            if (!(sender is IDedicatedServer server)) return;

            _logger.LogInformation("Server disposed, removing.");

            _ = _servers.TryRemove(server.Port, out _);
        }

        private async Task DiscoverProcesses()
        {
            const string portString = "port ";

            var armaServerProcesses = Process.GetProcesses()
                .Where(x => x.ProcessName.Contains(ArmaProcessName))
                .ToList();

            if (!armaServerProcesses.Any()) return;

            _logger.LogInformation($"Found {{count}} running {ArmaProcessName} processes.", armaServerProcesses.Count);

            var hangfireManager = _serviceProvider.GetService<IHangfireManager>();

            foreach (var armaServerProcess in armaServerProcesses)
            {
                var portStringIndex = armaServerProcess.MainWindowTitle.LastIndexOf(portString, StringComparison.InvariantCulture);
                var portIntString = armaServerProcess.MainWindowTitle.Substring(portStringIndex + portString.Length);
                var parseSuccess = int.TryParse(portIntString, out var port);

                if (!parseSuccess) continue;

                _logger.LogInformation("Found server running on port {port}.", port);

                var unknownModset = new Modset
                {
                    Name = "Unknown"
                };

                var server = CreateServer(port, unknownModset, armaServerProcess);
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var serverStatus = await server.GetServerStatusAsync(cancellationTokenSource.Token);
                if (serverStatus.Players == 0)
                {
                    _logger.LogInformation("Server is empty, disposing it.");
                    server.Dispose();
                }
                else
                {
                    _logger.LogInformation("Server has {playersCount} online, cannot be auto shutdown.", serverStatus.Players);
                    _servers.GetOrAdd(port, server);
                    hangfireManager.ScheduleJob<ServerStartupService>(x => x.ShutdownServer(port, false, CancellationToken.None));
                }
            }
        }

        /// <summary>
        /// TODO: Create factory for this, so the IServiceProvider dependency can be removed.
        /// </summary>
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
