using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    /// <summary>
    /// Performs updates of installed mods.
    /// </summary>
    public interface IModsUpdateService
    {
        Task<Result> UpdateModset(string modsetName, CancellationToken cancellationToken);

        Task<Result> UpdateModset(Modset modset, CancellationToken cancellationToken);

        Task UpdateMods(IEnumerable<Mod> mods, CancellationToken cancellationToken);

        /// <summary>
        ///     Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        Task UpdateAllMods(CancellationToken cancellationToken);
    }
}
