using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Mods;

namespace Arma.Server.Manager.Services {
    public class ModsUpdateService {
        public async Task Update(CancellationToken cancellationToken) {
            ISettings settings = new Settings();
            settings.LoadSettings();
            var modsManager = new ModsManager(settings);
            await modsManager.UpdateAllMods(cancellationToken);
        }
    }
}