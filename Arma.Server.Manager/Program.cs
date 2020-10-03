using Arma.Server.Config;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Manager.Mods;
using Arma.Server.Modset;

namespace Arma.Server.Manager {
    internal class Program {
        static void Main(string[] args) {
            ISettings settings = new Settings();
            settings.LoadSettings();
            var apiService = new ApiModsetClient(settings.ApiModsetsBaseUrl);
            ModsManager modsManager = new ModsManager(settings);
            IModset modset = apiService.GetModsetDataByName("default-test").ConvertForServer();
            modsManager.PrepareModset(modset);
            IModsetConfig modsetConfig = new ModsetConfig(settings, modset.Name);
            modsetConfig.LoadConfig();
            var server = new Server(settings, modsetConfig);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}