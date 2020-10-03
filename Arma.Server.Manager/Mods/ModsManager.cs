using Arma.Server.Config;
using Arma.Server.Manager.Steam;
using Arma.Server.Mod;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;

namespace Arma.Server.Manager.Mods {
    public class ModsManager {
        private readonly IClient _steamClient;
        private readonly IModsCache _modsCache;

        public ModsManager(ISettings settings) : this(new Client(settings), new ModsCache(settings)){}

        public ModsManager(IClient steamClient, IModsCache modsCache) {
            _steamClient = steamClient;
            _modsCache = modsCache;
        }

        public Result PrepareModset(IModset modset) 
            => InitializeMods(modset);

        private Result InitializeMods(IModset modset) {
            return CheckModsExist(modset.Mods)
                .TapIf(x => x.Any(), DownloadMods)
                .Bind(x => CheckModsUpdated(modset.Mods))
                .TapIf(x => x.Any(), UpdateMods);
        }

        public Result<IEnumerable<IMod>> CheckModsExist(IEnumerable<IMod> modsList) {
            var missingMods = modsList.Where(mod => !_modsCache.ModExists(mod));
            return Result.Success(missingMods);
        }

        public Result<IEnumerable<IMod>> CheckModsUpdated(IEnumerable<IMod> modsList) {
            var modsRequireUpdate = modsList.Where(ModRequiresUpdate);
            return Result.Success(modsRequireUpdate);
        }

        private bool ModRequiresUpdate(IMod mod)
            => false;

        private void DownloadMods(IEnumerable<IMod> missingMods) {
            _steamClient.Download(missingMods.Select(x => x.WorkshopId));
        }

        private void UpdateMods(IEnumerable<IMod> requiredUpdateMods) {
            _steamClient.Download(requiredUpdateMods.Select(x => x.WorkshopId));
        }
    }
}