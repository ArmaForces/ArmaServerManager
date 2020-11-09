using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Providers;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Services
{
    /// <summary>
    ///     
    /// </summary>
    public class ServerStartupService
    {
        private readonly IModsetProvider _modsetProvider;
        private readonly IModsUpdateService _modsUpdateService;

        public ServerStartupService(
            IModsetProvider modsetProvider,
            IModsUpdateService modsUpdateService)
        {
            _modsetProvider = modsetProvider;
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

            return Result.Success();
        }
    }
}
