using Arma.Server.Config;
using Arma.Server.Manager.Steam;
using Arma.Server.Mod;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arma.Server.Manager.Mods {
    public class ModsManager {
        private readonly ISettings _settings;

        private readonly string _modsPath;
        private IClient _steamClient;

        public ModsManager(ISettings settings) {
            _settings = settings;
            _modsPath = _settings.ModsDirectory;
        }

        public Result PrepareModset(IModset modset) 
            => InitializeMods(modset);

        private Result InitializeMods(IModset modset) {
            return CheckModsExist(modset.Mods)
                .TapIf(x => x.Any(), DownloadMods)
                .Bind(x => CheckModsUpdated(modset.Mods))
                .TapIf(x => x.Any(), UpdateMods);
        }

        public Result<IEnumerable<Mod.Mod>> CheckModsExist(IEnumerable<Mod.Mod> modsList) {
            var missingMods = modsList.Where(mod => ModExists(mod));
            return Result.Success(missingMods);
        }

        public Result<IEnumerable<Mod.Mod>> CheckModsUpdated(IEnumerable<Mod.Mod> modsList) {
            var modsRequireUpdate = modsList.Where(ModRequiresUpdate);
            return Result.Success(modsRequireUpdate);
        }

        private bool ModRequiresUpdate(IMod mod)
            => false;

        private bool ModExists(IMod mod)
            => ModExists(_modsPath, mod);

        public static bool ModExists(string modsDirectory, IMod mod)
            => ModExists(modsDirectory, mod.WorkshopId) ||
               ModExists(modsDirectory, mod.Name);

        public static bool ModExists(string modsDirectory, int itemId)
            => Directory.Exists(Path.Join(modsDirectory, itemId.ToString()));

        public static bool ModExists(string modsDirectory, string itemName)
            => Directory.Exists(Path.Join(modsDirectory, itemName)) ||
               Directory.Exists(Path.Join(modsDirectory, "@", itemName));

        private void EnsureClientCreated() {
            if (_steamClient is null)
                _steamClient = new Client(_settings);
        }

        private void DownloadMods(IEnumerable<Mod.Mod> missingMods) {
            EnsureClientCreated();
            _steamClient.Downloader.DownloadMods(missingMods.Select(x => (uint) x.WorkshopId));
        }

        private void UpdateMods(IEnumerable<Mod.Mod> requiredUpdateMods) {
            EnsureClientCreated();
            _steamClient.Downloader.DownloadMods(requiredUpdateMods.Select(x => (uint)x.WorkshopId));
        }
    }
}