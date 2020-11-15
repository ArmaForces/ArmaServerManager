using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Clients.Steam;
using Arma.Server.Mod;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Features.Mods
{
    /// <inheritdoc />
    public class ModsManager : IModsManager
    {
        private readonly IModsDownloader _modsDownloader;
        private readonly IModsCache _modsCache;

        /// <inheritdoc />
        public ModsManager(ISettings settings) : this(new ModsDownloader(new ContentDownloader(settings)), new ModsCache(settings))
        {
        }

        /// <inheritdoc cref="ModsManager" />
        /// <param name="modsDownloader">Client for mods download and updating.</param>
        /// <param name="modsCache">Installed mods cache.</param>
        public ModsManager(IModsDownloader modsDownloader, IModsCache modsCache)
        {
            _modsDownloader = modsDownloader;
            _modsCache = modsCache;
        }

        /// <inheritdoc />
        public async Task<Result> PrepareModset(IModset modset) => await InitializeMods(modset);

        /// <inheritdoc />
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await UpdateMods(_modsCache.Mods, cancellationToken);

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
            var modsRequireUpdate = modsList.Where(ModRequiresUpdate);
            return Result.Success(modsRequireUpdate);
        }

        public static ModsManager CreateModsManager(IServiceProvider serviceProvider)
            => new ModsManager(serviceProvider.GetService<IModsDownloader>(), serviceProvider.GetService<IModsCache>());

        private async Task<Result> InitializeMods(IModset modset)
            => await CheckModsExist(modset.Mods)
                .TapIf(x => x.Any(), x => DownloadMods(x, CancellationToken.None))
                .Bind(x => CheckModsUpdated(modset.Mods))
                .TapIf(x => x.Any(), x => UpdateMods(x, CancellationToken.None));

        private bool ModRequiresUpdate(IMod mod) => false;

        /// <summary>
        ///     Invokes <see cref="ISteamClient" /> to download given list of mods.
        /// </summary>
        /// <param name="missingMods">Mods to download.</param>
        /// ///
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods download safe cancelling.</param>
        private async Task<List<Result>> DownloadMods(IEnumerable<IMod> missingMods, CancellationToken cancellationToken)
            => await _modsDownloader.Download(missingMods.Select(x => x.WorkshopId), cancellationToken);

        /// <summary>
        ///     Invokes <see cref="ISteamClient" /> to update given list of mods.
        /// </summary>
        /// <param name="requiredUpdateMods">Mods to update.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods update safe cancelling.</param>
        private async Task<List<Result>> UpdateMods(IEnumerable<IMod> requiredUpdateMods, CancellationToken cancellationToken)
            => await _modsDownloader.Download(requiredUpdateMods.Select(x => x.WorkshopId), cancellationToken);
    }
}
