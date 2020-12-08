using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Arma.Server.Config;
using Arma.Server.Features.Server;
using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Services;
using Arma.Server.Modset;
using Arma.Server.Providers.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arma.Server.Manager.Providers.Server
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

        private void DiscoverProcesses()
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

                var unknownModset = new Modset.Modset
                {
                    Name = "Unknown"
                };

                var server = CreateServer(port, unknownModset, armaServerProcess);
                if (server.ServerStatus.Players == 0)
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
