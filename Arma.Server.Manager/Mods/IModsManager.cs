using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Mod;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Mods {
    /// <summary>
    /// Prepares modset by downloading missing mods and updating outdated mods.
    /// </summary>
    public interface IModsManager {
        /// <inheritdoc cref="IModsManager"/>
        /// <param name="modset">Modset to prepare.</param>
        /// <returns><see cref="Result.Success"/> if all mods from <see cref="IModset"/> are downloaded and up to date.</returns>
        Result PrepareModset(IModset modset);

        /// <summary>
        /// Checks if all mods from given list exist.
        /// </summary>
        /// <param name="modsList">List of mods to check.</param>
        /// <returns><see cref="Result{T}"/> with missing mods.</returns>
        Result<IEnumerable<IMod>> CheckModsExist(IEnumerable<IMod> modsList);

        /// <summary>
        /// Checks if all mods from given list are up to date.
        /// </summary>
        /// <param name="modsList">List of mods to check.</param>
        /// <returns><see cref="Result{T}"/> with outdated mods.</returns>
        Result<IEnumerable<IMod>> CheckModsUpdated(IEnumerable<IMod> modsList);

        /// <summary>
        /// Updates all installed mods.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> used for task cancellation.</param>
        Task UpdateAllMods(CancellationToken cancellationToken);
    }
}