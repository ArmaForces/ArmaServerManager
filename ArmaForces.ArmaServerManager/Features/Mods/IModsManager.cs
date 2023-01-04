using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Mods {
    /// <summary>
    /// Prepares modset by downloading missing mods and updating outdated mods.
    /// </summary>
    public interface IModsManager {
        /// <inheritdoc cref="IModsManager"/>
        /// <param name="modset">Modset to prepare.</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="Result.Success"/> if all mods from <see cref="ArmaForces.Arma.Server.Features.Modsets.Modset"/> are downloaded and up to date.</returns>
        Task<Result> PrepareModset(Modset modset, CancellationToken cancellationToken);

        /// <summary>
        /// Checks if all mods from given list exist.
        /// </summary>
        /// <param name="modsList">List of mods to check.</param>
        /// <returns><see cref="Result{T}"/> with missing mods.</returns>
        Result<IEnumerable<Mod>> CheckModsExist(IEnumerable<Mod> modsList);

        /// <summary>
        /// Checks if all mods from given list are up to date.
        /// </summary>
        /// <param name="modsList">List of mods to check.</param>
        /// <returns><see cref="Result{T}"/> with outdated mods.</returns>
        Task<Result<List<Mod>>> CheckModsUpdated(IReadOnlyCollection<Mod> modsList, CancellationToken cancellationToken);

        /// <summary>
        /// Updates given <paramref name="mods"/>.
        /// </summary>
        /// <param name="mods">List of mods to update.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> used for task cancellation.</param>
        Task UpdateMods(IReadOnlyCollection<Mod> mods, CancellationToken cancellationToken);

        /// <summary>
        /// Updates all installed mods.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> used for task cancellation.</param>
        Task UpdateAllMods(CancellationToken cancellationToken);
    }
}