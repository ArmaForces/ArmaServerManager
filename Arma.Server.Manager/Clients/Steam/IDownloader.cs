﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Clients.Steam {
    /// <summary>
    /// Handles downloading/updating mods and arma server.
    /// </summary>
    public interface IDownloader {
        /// <summary>
        /// Downloads arma server.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for safe download abort.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task DownloadArmaServer(CancellationToken cancellationToken);

        /// <summary>
        /// Downloads mod with given <paramref name="itemId"/>.
        /// </summary>
        /// <param name="itemId">ID of item to download</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for safe download abort.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task DownloadMod(int itemId, CancellationToken cancellationToken);

        /// <summary>
        /// Downloads mods one by one from list of <paramref name="itemsIds"/>.
        /// </summary>
        /// <param name="itemsIds">IDs of items to download</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for safe download abort.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task DownloadMods(IEnumerable<int> itemsIds, CancellationToken cancellationToken);
    }
}