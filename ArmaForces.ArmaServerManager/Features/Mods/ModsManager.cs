using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using ArmaForces.ArmaServerManager.Features.Steam.RemoteStorage;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    /// <inheritdoc />
    internal class ModsManager : IModsManager
    {
        private readonly IContentDownloader _contentDownloader;
        private readonly IContentVerifier _contentVerifier;
        private readonly IModsCache _modsCache;
        private readonly ISteamRemoteStorage _steamRemoteStorage;
        private readonly ILogger<ModsManager> _logger;

        /// <inheritdoc cref="ModsManager" />
        /// <param name="contentDownloader">Client for mods download and updating.</param>
        /// <param name="contentVerifier">Client for verifying whether mods are up to date and correct.</param>
        /// <param name="modsCache">Installed mods cache.</param>
        /// <param name="steamRemoteStorage">Steam remote storage for quick access to mods metadata on Workshop.</param>
        /// <param name="logger">Logger.</param>
        public ModsManager(
            IContentDownloader contentDownloader,
            IContentVerifier contentVerifier,
            IModsCache modsCache,
            ISteamRemoteStorage steamRemoteStorage,
            ILogger<ModsManager> logger)
        {
            _contentDownloader = contentDownloader;
            _contentVerifier = contentVerifier;
            _modsCache = modsCache;
            _steamRemoteStorage = steamRemoteStorage;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<UnitResult<IError>> PrepareModset(Modset modset, CancellationToken cancellationToken)
            => await CheckUpdatesAndDownloadMods(modset.ActiveMods, cancellationToken)
                .Tap(() => _logger.LogInformation("Preparation of {ModsetName} modset finished", modset.Name));

        /// <inheritdoc />
        public async Task UpdateMods(IReadOnlyCollection<Mod> mods, CancellationToken cancellationToken)
            => await CheckUpdatesAndDownloadMods(mods, cancellationToken)
                .Tap(() => _logger.LogInformation("Update of {Count} mods finished", mods.Count));

        /// <inheritdoc />
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await CheckUpdatesAndDownloadMods(_modsCache.Mods, cancellationToken)
                .Tap(() => _logger.LogInformation("Update of all mods finished"));

        /// <inheritdoc />
        public Result<IEnumerable<Mod>, IError> CheckModsExist(IEnumerable<Mod> modsList)
        {
            var missingMods = modsList
                .ToAsyncEnumerable()
                .WhereAwait(async mod => !await _modsCache.ModExists(mod))
                .ToEnumerable();
            
            return missingMods.ToResult();
        }

        /// <inheritdoc />
        public async Task<Result<List<Mod>, IError>> CheckModsUpdated(IReadOnlyCollection<Mod> modsList, CancellationToken cancellationToken)
        {
            if (modsList.IsEmpty())
            {
                return new List<Mod>();
            }
            
            var workshopMods = modsList
                .Where(x => x.Source == ModSource.SteamWorkshop)
                .ToList();

            var workshopModIds = workshopMods
                .Select(x => x.WorkshopId)
                .Where(x => x.HasValue)
                .Select(x => (ulong)x!.Value)
                .ToList();

            var publishedFileDetails = await _steamRemoteStorage.GetPublishedFileDetails(workshopModIds, cancellationToken);
            
            var modsToUpdate = _modsCache.Mods
                .Join(publishedFileDetails.Value, mod => mod.WorkshopId, fileDetails => fileDetails.PublishedFileId,
                    (mod, details) => new {mod, details})
                .Where(x => x.mod.LastUpdatedAt < x.details.LastUpdatedAt)
                .Select(x => x.mod)
                .ToList();

            var modsNotInCache = workshopMods
                .Where(x => _modsCache.Mods.NotContains(x));
            
            return modsNotInCache.Concat(modsToUpdate)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<Result<List<Mod>, IError>> VerifyMods(IReadOnlyCollection<Mod> modsList, CancellationToken cancellationToken)
        {
            if (modsList.IsEmpty())
            {
                return new List<Mod>();
            }
            
            var modsRequireUpdate = new ConcurrentBag<Mod>();

            await foreach (var mod in modsList.Where(x => x.Source == ModSource.SteamWorkshop)
                .ToAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                await _contentVerifier.ItemIsUpToDate(mod.AsContentItem(), cancellationToken)
                    .TapError(() => modsRequireUpdate.Add(mod));
            }
            
            return modsRequireUpdate.ToList();
        }
        
        /// <summary>
        /// Checks which mods require updates, downloads them and updates in cache.
        /// </summary>
        /// <param name="modsToDownload">Mods to download.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods download safe cancelling.</param>
        private async Task<UnitResult<IError>> CheckUpdatesAndDownloadMods(IEnumerable<Mod> modsToDownload, CancellationToken cancellationToken)
        {
            return await CheckModsUpdated(modsToDownload.ToList(), cancellationToken)
                .Bind(modsMissingOrOutdated => DownloadMods(modsMissingOrOutdated, cancellationToken));
        }

        private async Task<UnitResult<IError>> DownloadMods(
            IReadOnlyCollection<Mod> modsToDownload,
            CancellationToken cancellationToken)
        {
            if (modsToDownload.IsEmpty())
            {
                return UnitResult.Success<IError>();
            }
            
            var downloadResults = await _contentDownloader.DownloadOrUpdateMods(
                modsToDownload,
                cancellationToken);

            var successfullyDownloadedMods = downloadResults
                .Where(x => x.IsSuccess)
                .Select(x => x.Value)
                .ToList();

            await _modsCache.AddOrUpdateModsInCache(successfullyDownloadedMods);

            return downloadResults.Combine()
                .MapError(IError (x) => new Error(x, ManagerErrorCode.ModsDownloadFailed));
        }
    }
}
