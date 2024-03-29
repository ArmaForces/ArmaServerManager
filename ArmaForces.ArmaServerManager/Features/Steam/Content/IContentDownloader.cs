﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    /// <summary>
    ///     Handles downloading/updating mods and arma server.
    /// </summary>
    public interface IContentDownloader
    {
        /// <summary>
        /// Downloads <paramref name="mods"/>.
        /// </summary>
        /// <param name="mods">Mods to download.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Download result for each mod.</returns>
        Task<List<Result<Mod>>> DownloadOrUpdateMods(
            IReadOnlyCollection<Mod> mods,
            CancellationToken cancellationToken);
    }
}
