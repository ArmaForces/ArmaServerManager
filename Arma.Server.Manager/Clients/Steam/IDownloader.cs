using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Clients.Steam {
    /// <summary>
    /// Handles downloading/updating mods and arma server.
    /// </summary>
    public interface IDownloader {
        /// <summary>
        /// Downloads arma server.
        /// </summary>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task DownloadArmaServer();

        /// <summary>
        /// Downloads mod with given <paramref name="itemId"/>.
        /// </summary>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task DownloadMod(int itemId);

        /// <summary>
        /// Downloads mods one by one from list of <paramref name="itemsIds"/>.
        /// </summary>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task DownloadMods(IEnumerable<int> itemsIds);
    }
}