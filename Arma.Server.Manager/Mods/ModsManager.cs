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

namespace Arma.Server.Manager.Mods
{
    /// <inheritdoc />
    public class ModsManager : IModsManager
    {
        private readonly IModsCache _modsCache;
        private readonly ISteamClient _steamClient;

        /// <inheritdoc />
        public ModsManager(ISettings settings) : this(new SteamClient(settings), new ModsCache(settings))
        {
        }

        /// <inheritdoc cref="ModsManager" />
        /// <param name="steamClient">Steam client for mods download and updating.</param>
        /// <param name="modsCache">Installed mods cache.</param>
        public ModsManager(ISteamClient steamClient, IModsCache modsCache)
        {
            _steamClient = steamClient;
            _modsCache = modsCache;
        }

        /// <inheritdoc />
        public Result PrepareModset(IModset modset) => InitializeMods(modset);

        /// <inheritdoc />
        public async Task UpdateAllMods(CancellationToken cancellationToken)
            => await UpdateMods(_modsCache.Mods, cancellationToken);

        /// <inheritdoc />
        public Result<IEnumerable<IMod>> CheckModsExist(IEnumerable<IMod> modsList)
        {
            var missingMods = modsList.Where(mod => !_modsCache.ModExists(mod));
            return Result.Success(missingMods);
        }

        /// <inheritdoc />
        public Result<IEnumerable<IMod>> CheckModsUpdated(IEnumerable<IMod> modsList)
        {
            var modsRequireUpdate = modsList.Where(ModRequiresUpdate);
            return Result.Success(modsRequireUpdate);
        }

        public static ModsManager CreateModsManager(IServiceProvider serviceProvider)
            => new ModsManager(serviceProvider.GetService<ISteamClient>(), serviceProvider.GetService<IModsCache>());

        private Result InitializeMods(IModset modset)
            => CheckModsExist(modset.Mods)
                .TapIf(x => x.Any(), async x => await DownloadMods(x, CancellationToken.None))
                .Result
                .Bind(x => CheckModsUpdated(modset.Mods))
                .TapIf(x => x.Any(), async x => await UpdateMods(x, CancellationToken.None))
                .Result;

        private bool ModRequiresUpdate(IMod mod) => false;

        /// <summary>
        ///     Invokes <see cref="ISteamClient" /> to download given list of mods.
        /// </summary>
        /// <param name="missingMods">Mods to download.</param>
        /// ///
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods download safe cancelling.</param>
        private async Task DownloadMods(IEnumerable<IMod> missingMods, CancellationToken cancellationToken)
            => _steamClient.Download(missingMods.Select(x => x.WorkshopId), cancellationToken);

        /// <summary>
        ///     Invokes <see cref="ISteamClient" /> to update given list of mods.
        /// </summary>
        /// <param name="requiredUpdateMods">Mods to update.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for mods update safe cancelling.</param>
        private async Task UpdateMods(IEnumerable<IMod> requiredUpdateMods, CancellationToken cancellationToken)
            => await _steamClient.Download(requiredUpdateMods.Select(x => x.WorkshopId), cancellationToken);
    }
}
