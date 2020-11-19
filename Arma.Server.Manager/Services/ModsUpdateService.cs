using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Manager.Providers;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Services
{
    /// <summary>
    ///     Schedules recurring job for updating all cached mods.
    /// </summary>
    public class ModsUpdateService : IModsUpdateService
    {
        private readonly IModsManager _modsManager;
        private readonly IModsetProvider _modsetProvider;

        public ModsUpdateService(IModsManager modsManager, IModsetProvider modsetProvider)
        {
            _modsManager = modsManager;
            _modsetProvider = modsetProvider;
        }

        public async Task<Result> UpdateModset(string modsetName, CancellationToken cancellationToken)
        {
            return await _modsetProvider.GetModsetByName(modsetName)
                .Bind(x => UpdateModset(x, cancellationToken));
        }

        public async Task<Result> UpdateModset(IModset modset, CancellationToken cancellationToken)
        {
            return await _modsManager.PrepareModset(modset, cancellationToken);
        }

        /// <summary>
        ///     Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        [MaximumConcurrentExecutions(1)]
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await _modsManager.UpdateAllMods(cancellationToken);

        public static ModsUpdateService CreateModsUpdateService(IServiceProvider serviceProvider)
            => new ModsUpdateService(
                serviceProvider.GetService<IModsManager>(),
                serviceProvider.GetService<IModsetProvider>());
    }
}
