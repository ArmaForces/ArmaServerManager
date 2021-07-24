﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    /// <inheritdoc />
    internal class ModsManager : IModsManager
    {
        private readonly IContentDownloader _contentDownloader;
        private readonly IContentVerifier _contentVerifier;
        private readonly IModsCache _modsCache;

        /// <inheritdoc cref="ModsManager" />
        /// <param name="contentDownloader">Client for mods download and updating.</param>
        /// <param name="contentVerifier">Client for verifying whether mods are up to date and correct.</param>
        /// <param name="modsCache">Installed mods cache.</param>
        public ModsManager(
            IContentDownloader contentDownloader,
            IContentVerifier contentVerifier,
            IModsCache modsCache)
        {
            _contentDownloader = contentDownloader;
            _contentVerifier = contentVerifier;
            _modsCache = modsCache;
        }

        /// <inheritdoc />
        public async Task<Result> PrepareModset(IModset modset, CancellationToken cancellationToken)
            => await CheckUpdatesAndDownloadMods(modset.Mods, cancellationToken);

        /// <inheritdoc />
        public async Task UpdateMods(IEnumerable<IMod> mods, CancellationToken cancellationToken)
            => await CheckUpdatesAndDownloadMods(mods, cancellationToken);

        /// <inheritdoc />
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await CheckUpdatesAndDownloadMods(_modsCache.Mods, cancellationToken);

        /// <inheritdoc />
        public Result<IEnumerable<IMod>> CheckModsExist(IEnumerable<IMod> modsList)
        {
            var missingMods = modsList
                .ToAsyncEnumerable()
                .WhereAwait(async mod => !await _modsCache.ModExists(mod))
                .ToEnumerable();
            return Result.Success(missingMods);
        }

        /// <inheritdoc />
        public async Task<Result<List<IMod>>> CheckModsUpdated(IReadOnlyCollection<IMod> modsList, CancellationToken cancellationToken)
        {
            if (modsList.IsEmpty())
            {
                return Result.Success(new List<IMod>());
            }
            
            var modsRequireUpdate = new ConcurrentBag<IMod>();

            await foreach (var mod in modsList.Where(x => x.Source == ModSource.SteamWorkshop)
                .ToAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                await _contentVerifier.ItemIsUpToDate(mod.AsContentItem(), cancellationToken)
                    .OnFailure(() => modsRequireUpdate.Add(mod));
            }
            
            return Result.Success(modsRequireUpdate.ToList());
        }
        
        /// <summary>
        /// Checks which mods require updates, downloads them and updates in cache.
        /// </summary>
        /// <param name="modsToDownload">Mods to download.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods download safe cancelling.</param>
        private async Task<Result> CheckUpdatesAndDownloadMods(IEnumerable<IMod> modsToDownload, CancellationToken cancellationToken)
        {
            return await CheckModsUpdated(modsToDownload.ToList(), cancellationToken)
                .Bind(modsMissingOrOutdated => DownloadMods(modsMissingOrOutdated, cancellationToken));
        }

        private async Task<Result> DownloadMods(
            IReadOnlyCollection<IMod> modsToDownload,
            CancellationToken cancellationToken)
        {
            if (modsToDownload.IsEmpty())
            {
                return Result.Success();
            }
            
            var downloadResults = await _contentDownloader.DownloadOrUpdateMods(
                modsToDownload,
                cancellationToken);

            var successfullyDownloadedMods = downloadResults
                .Where(x => x.IsSuccess)
                .Select(x => x.Value)
                .ToList();

            await _modsCache.AddOrUpdateModsInCache(successfullyDownloadedMods)
                .Tap(() => _modsCache.SaveCache());

            return downloadResults.Combine();
        }
    }
}
