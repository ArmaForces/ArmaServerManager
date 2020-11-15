using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Modset;
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

        public ModsUpdateService(IModsManager modsManager)
        {
            _modsManager = modsManager;
        }

        public async Task UpdateModset(IModset modset, CancellationToken cancellationToken)
        {
            await _modsManager.PrepareModset(modset, cancellationToken);
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
            => new ModsUpdateService(serviceProvider.GetService<IModsManager>());
    }
}
