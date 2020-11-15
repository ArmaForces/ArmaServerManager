using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Clients.Steam
{
    /// <summary>
    ///     Handles downloading/updating mods and arma server.
    /// </summary>
    public interface IContentDownloader
    {
        Task<List<Result>> DownloadOrUpdate(IEnumerable<KeyValuePair<int, ItemType>> items, CancellationToken cancellationToken);

        /// <summary>
        ///     Downloads <paramref name="itemType"/> with given <paramref name="itemId" />.
        /// </summary>
        /// <param name="itemId">ID of item to download.</param>
        /// <param name="itemType">Type of item to download.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> for safe download abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        Task<Result> DownloadOrUpdate(
            int itemId,
            ItemType itemType,
            CancellationToken cancellationToken);
    }
}
