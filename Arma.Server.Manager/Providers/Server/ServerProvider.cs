using System;
using System.Collections.Concurrent;
using Arma.Server.Config;
using Arma.Server.Features.Server;
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

        public ServerProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public IDedicatedServer GetServer(int port, IModset modset)
            => _servers.GetOrAdd(port, CreateServer(port, modset));

        public static ServerProvider CreateServerProvider(IServiceProvider serviceProvider)
            => new ServerProvider(serviceProvider);

        private void OnServerDisposed(object sender, EventArgs e)
        {
            if (!(sender is IDedicatedServer server)) return;

            _ = _servers.TryRemove(server.Port, out _);
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
    }
}
