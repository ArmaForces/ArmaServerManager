using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Extensions;
using Arma.Server.Manager.Features.Steam;
using Arma.Server.Manager.Features.Steam.Content;
using Arma.Server.Mod;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Features.Mods
{
    /// <inheritdoc />
    public class ModsManager : IModsManager
    {
        private readonly IContentDownloader _contentDownloader;
        private readonly IContentVerifier _contentVerifier;
        private readonly IModsCache _modsCache;

        /// <inheritdoc cref="ModsManager" />
        /// <param name="contentDownloader">Client for mods download and updating.</param>
        /// <param name="contentVerifier">Client for verifying whether mods are up to date and correct.</param>
        /// <param name="modsCache">Installed mods cache.</param>
        public ModsManager(IContentDownloader contentDownloader, IContentVerifier contentVerifier, IModsCache modsCache)
        {
            _contentDownloader = contentDownloader;
            _contentVerifier = contentVerifier;
            _modsCache = modsCache;
        }

        /// <inheritdoc />
        public async Task<Result> PrepareModset(IModset modset, CancellationToken cancellationToken)
        {
            var results = await DownloadMods(modset.Mods, cancellationToken);

            return results;
        }

        /// <inheritdoc />
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await DownloadMods(_modsCache.Mods, cancellationToken);

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
        public Result<IEnumerable<IMod>> CheckModsUpdated(IEnumerable<IMod> modsList)
        {
            var modsRequireUpdate = modsList
                .ToAsyncEnumerable()
                .WhereAwait(async x => (await _contentVerifier.ItemIsUpToDate(x.AsContentItem(), CancellationToken.None)).IsFailure)
                .ToEnumerable();
            return Result.Success(modsRequireUpdate);
        }

        public static ModsManager CreateModsManager(IServiceProvider serviceProvider)
            => new ModsManager(
                serviceProvider.GetService<IContentDownloader>(),
                serviceProvider.GetService<IContentVerifier>(),
                serviceProvider.GetService<IModsCache>());

        /// <summary>
        ///     Invokes <see cref="ISteamClient" /> to download given list of mods.
        /// </summary>
        /// <param name="modsToDownload">Mods to download.</param>
        /// ///
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods download safe cancelling.</param>
        private async Task<Result> DownloadMods(IEnumerable<IMod> modsToDownload, CancellationToken cancellationToken)
        {
            var downloadResults = await _contentDownloader.DownloadOrUpdateMods(modsToDownload, cancellationToken);

            var successfullyDownloadedMods = downloadResults
                .Where(x => x.IsSuccess)
                .Select(x => x.Value);

            await _modsCache.AddOrUpdateCache(successfullyDownloadedMods);

            return downloadResults.Combine();
        }
    }
}
