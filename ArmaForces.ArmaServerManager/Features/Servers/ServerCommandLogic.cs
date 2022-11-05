using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Configuration.Providers;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.ArmaServerManager.Features.Servers.Providers;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Servers
{
    internal class ServerCommandLogic : IServerCommandLogic
    {
        private readonly IServerProvider _serverProvider;
        private readonly IDedicatedServerFactory _dedicatedServerFactory;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly IModsetConfigurationProvider _modsetConfigurationProvider;
        private readonly ILogger<ServerCommandLogic> _logger;

        public ServerCommandLogic(
            IServerProvider serverProvider,
            IDedicatedServerFactory dedicatedServerFactory,
            IArmaProcessFactory armaProcessFactory,
            IModsetConfigurationProvider modsetConfigurationProvider,
            ILogger<ServerCommandLogic> logger)
        {
            _serverProvider = serverProvider;
            _dedicatedServerFactory = dedicatedServerFactory;
            _armaProcessFactory = armaProcessFactory;
            _modsetConfigurationProvider = modsetConfigurationProvider;
            _logger = logger;
        }

        public Result<IDedicatedServer> StartServer(int port, int headlessClients, Modset modset)
        {
            var newServerCreated = false;
            var server = _serverProvider.GetOrAddServer(port, x => CreateServer(x, modset, headlessClients, ref newServerCreated));

            if (newServerCreated) return server.Start().Map(() => server);

            return server.Modset.Equals(modset) && server.HeadlessClientsConnected == headlessClients
                ? Result.Success(server)
                : Result.Failure<IDedicatedServer>($"Expected to get server with {modset.Name} modset with {modset.Mods.Count} mods and {headlessClients} HCs on port {port} but found {server.Modset.Name} with {server.Modset.Mods.Count} mods and {server.HeadlessClientsConnected} HCs.");
        }

        public async Task<Result> ShutdownServer(int port, bool force = false, CancellationToken? cancellationToken = null)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null || server.IsServerStopped) return Result.Success();

            var serverStatus = await server.GetServerStatusAsync(cancellationToken ?? CancellationToken.None);

            if (serverStatus.Players.HasValue && serverStatus.Players != 0 && !force)
            {
                return Result.Failure($"Server cannot be shut down, there are {serverStatus.Players} online.");
            }

            return await server.Shutdown();
        }

        public async Task<Result> SetHeadlessClients(int port, int desiredHcCount)
        {
            using var _ = _logger.BeginScope(new KeyValuePair<string, object>("Port", port));
            
            _logger.LogTrace("Requested {Count} headless clients for server on port {Port}", desiredHcCount, port);
            var server = _serverProvider.GetServer(port);
            
            if (server is null)
            {
                _logger.LogInformation("Server doesn't exist on port {Port}", port);
                return Result.Failure($"Server doesn't exist on port {port}");
            }

            var headlessClientsConnected = server.HeadlessClientsConnected;
            _logger.LogTrace("Server on port {Port} has {Count} headless clients connected", port, headlessClientsConnected);
            
            if (headlessClientsConnected > desiredHcCount)
            {
                var clientsToRemove = server.HeadlessClientsConnected - desiredHcCount;
                _logger.LogDebug("Stopping {Count} headless clients from server on port {Port}", clientsToRemove, port);
                return await server.RemoveHeadlessClients(clientsToRemove);
            }

            var clientsToAdd = desiredHcCount - headlessClientsConnected;
            _logger.LogDebug("Starting additional {Count} headless clients for server on port {Port}", clientsToAdd, port);
            
            var createdClients = _armaProcessFactory.CreateHeadlessClients(
                port: port,
                modset: server.Modset,
                modsetConfig: _modsetConfigurationProvider.GetModsetConfig(server.Modset.Name),
                numberOfHeadlessClients: clientsToAdd);

            return server.AddAndStartHeadlessClients(createdClients);
        }

        // Value of 'newServerCreated' is used in different method 
        // ReSharper disable once RedundantAssignment
        private IDedicatedServer CreateServer(int port, Modset modset, int numberOfHeadlessClients, ref bool newServerCreated)
        {
            var server = _dedicatedServerFactory.CreateDedicatedServer(port, modset, numberOfHeadlessClients);

            server.OnServerShutdown += OnServerDisposed;

            newServerCreated = true;
            
            return server;
        }
        
        private Task OnServerDisposed(IDedicatedServer dedicatedServer)
        {
            _logger.LogDebug(
                "Server with {ModsetName} disposed on port {Port}, removing",
                dedicatedServer.Modset.Name,
                dedicatedServer.Port);

            _serverProvider.TryRemoveServer(dedicatedServer);
            
            return Task.CompletedTask;
        }
    }
}