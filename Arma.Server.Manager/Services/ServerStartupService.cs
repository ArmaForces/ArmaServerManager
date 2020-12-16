﻿using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Providers;
using Arma.Server.Manager.Providers.Server;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Services
{
    /// <summary>
    /// TODO: create documentation
    /// </summary>
    public class ServerStartupService : IServerStartupService
    {
        private const int Port = 2302;

        private readonly IModsetProvider _modsetProvider;
        private readonly IServerProvider _serverProvider;
        private readonly IModsUpdateService _modsUpdateService;

        public ServerStartupService(
            IModsetProvider modsetProvider,
            IServerProvider serverProvider,
            IModsUpdateService modsUpdateService)
        {
            _modsetProvider = modsetProvider;
            _serverProvider = serverProvider;
            _modsUpdateService = modsUpdateService;
        }
        
        public async Task<Result> StartServer(string modsetName, CancellationToken cancellationToken)
        {
            return await _modsetProvider.GetModsetByName(modsetName)
                .Bind(modset => StartServer(modset, cancellationToken));
        }

        public async Task<Result> StartServer(IModset modset, CancellationToken cancellationToken)
        {
            await _modsUpdateService.UpdateModset(modset, cancellationToken);

            var server = _serverProvider.GetServer(Port, modset);

            return server.IsServerStopped
                ? server.Start()
                : server.Modset == modset 
                    ? Result.Success()
                    : Result.Failure($"Server is already running with {server.Modset.Name} modset on port {Port}.");
        }

        public async Task<Result> ShutdownServer(int port, bool force, CancellationToken cancellationToken)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null) return Result.Success();

            var serverStatus = await server.GetServerStatusAsync(cancellationToken);

            if (serverStatus.Players != 0 && !force)
            {
                return Result.Failure($"Server cannot be shut down, there are {serverStatus.Players} online.");
            }

            server.Shutdown();
            return Result.Success();
        }
    }
}
