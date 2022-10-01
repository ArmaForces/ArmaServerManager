using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets;
using CSharpFunctionalExtensions;
using Hangfire;

namespace ArmaForces.ArmaServerManager.Services
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

        public async Task UpdateMods(IEnumerable<IMod> mods, CancellationToken cancellationToken)
        {
            await _modsManager.UpdateMods(mods.ToList(), cancellationToken);
        }

        /// <summary>
        ///     Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        [MaximumConcurrentExecutions(1)]
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await _modsManager.UpdateAllMods(cancellationToken);
    }
}
