using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Providers;
using Arma.Server.Manager.Providers.Server;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Services
{
    /// <summary>
    ///     
    /// </summary>
    public class ServerStartupService
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

        public static ServerStartupService CreateServerStartupService(IServiceProvider serviceProvider) 
            => new ServerStartupService(
                serviceProvider.GetService<IModsetProvider>(),
                serviceProvider.GetService<IServerProvider>(),
                serviceProvider.GetService<ModsUpdateService>());

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
    }
}
