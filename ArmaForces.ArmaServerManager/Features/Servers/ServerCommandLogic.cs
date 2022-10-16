﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Features.Modsets;
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
        private readonly ILogger<ServerCommandLogic> _logger;

        public ServerCommandLogic(
            IServerProvider serverProvider,
            IDedicatedServerFactory dedicatedServerFactory,
            ILogger<ServerCommandLogic> logger)
        {
            _serverProvider = serverProvider;
            _dedicatedServerFactory = dedicatedServerFactory;
            _logger = logger;
        }

        public Result<IDedicatedServer> StartServer(int port, int headlessClients, IModset modset)
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

        // Value of 'newServerCreated' is used in different method 
        // ReSharper disable once RedundantAssignment
        private IDedicatedServer CreateServer(int port, IModset modset, int numberOfHeadlessClients, ref bool newServerCreated)
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