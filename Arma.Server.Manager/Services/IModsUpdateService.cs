using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Services
{
    public interface IModsUpdateService
    {
        Task<Result> UpdateModset(string modsetName, CancellationToken cancellationToken);

        Task<Result> UpdateModset(IModset modset, CancellationToken cancellationToken);

        /// <summary>
        ///     Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        Task UpdateAllMods(CancellationToken cancellationToken);
    }
}
