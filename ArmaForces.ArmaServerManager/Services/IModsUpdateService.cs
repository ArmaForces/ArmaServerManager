using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    public interface IModsUpdateService
    {
        Task<Result> UpdateModset(string modsetName, CancellationToken cancellationToken);

        Task<Result> UpdateModset(IModset modset, CancellationToken cancellationToken);

        Task UpdateMods(IEnumerable<IMod> mods, CancellationToken cancellationToken);

        /// <summary>
        ///     Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        Task UpdateAllMods(CancellationToken cancellationToken);
    }
}
