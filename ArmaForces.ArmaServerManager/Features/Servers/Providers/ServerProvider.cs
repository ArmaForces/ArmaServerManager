using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Servers.Providers
{
    internal class ServerProvider : IServerProvider
    {
        private readonly ConcurrentDictionary<int, IDedicatedServer> _servers =
            new ConcurrentDictionary<int, IDedicatedServer>();

        private readonly ILogger<ServerProvider> _logger;

        public ServerProvider(Dictionary<int, IDedicatedServer> servers, ILogger<ServerProvider> logger)
        {
            _logger = logger;
            InitializeServersDictionary(servers);
        }

        public IDedicatedServer? GetServer(int port)
            => _servers.TryGetValue(port, out var server)
                ? server
                : null;

        public IDedicatedServer GetOrAddServer(int port, Func<int, IDedicatedServer> serverFactory)
            => _servers.GetOrAdd(port, serverFactory);

        public List<IDedicatedServer> GetServers() => _servers.Values.ToList();

        public Result TryRemoveServer(IDedicatedServer dedicatedServer)
        {
            var serverRemoved = _servers.TryRemove(new KeyValuePair<int, IDedicatedServer>(dedicatedServer.Port, dedicatedServer));

            if (serverRemoved)
            {
                _logger.LogDebug("Server removed on port {Port}", dedicatedServer.Port);
                return Result.Success();
            }

            _logger.LogDebug("Server not removed on port {Port}. Different server is already running", dedicatedServer.Port);
            return Result.Failure($"Different server is already running on port {dedicatedServer.Port}.");
        }

        /// <summary>
        /// Creates <see cref="ConcurrentDictionary{TKey,TValue}"/> and 
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        private void InitializeServersDictionary(
            Dictionary<int, IDedicatedServer> servers)
        {
            foreach (var (port, server) in servers)
            {
                _ = _servers.TryAdd(port, server);

                server.OnServerShutdown += OnServerDisposed;
                
                if (server.IsServerStopped)
                {
                    _ = _servers.TryRemove(new KeyValuePair<int, IDedicatedServer>(port, server));
                }
            }
        }

        private Task OnServerDisposed(IDedicatedServer dedicatedServer)
        {
            _logger.LogDebug(
                "Server with {ModsetName} disposed on port {Port}, removing",
                dedicatedServer.Modset.Name,
                dedicatedServer.Port);

            TryRemoveServer(dedicatedServer);
            
            return Task.CompletedTask;
        }
    }
}
