using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Clients.Steam {
    /// <summary>
    /// Handles connection to Steam Servers and downloading of mods.
    /// </summary>
    public interface ISteamClient {
        /// <summary>
        /// Connects to Steam Servers.
        /// </summary>
        /// /// <param name="cancellationToken"><see cref="CancellationToken"/> used for safe connection aborting.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task Connect(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects from Steam Servers.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Starts download of given <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">Item to download.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> used for safe download cancellation.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task Download(int itemId, CancellationToken cancellationToken);

        /// <summary>
        /// Starts download of given <paramref name="itemsIds"/>.
        /// </summary>
        /// <param name="itemsIds">List of items to download.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> used for safe download cancellation.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task Download(IEnumerable<int> itemsIds, CancellationToken cancellationToken);
    }
}