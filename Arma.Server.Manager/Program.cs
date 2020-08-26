using Arma.Server.Config;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Modset;

namespace Arma.Server.Manager {
    internal class Program {
        static void Main(string[] args) {
            ISettings settings = new Settings();
            settings.LoadSettings();
            var baseUrl = "https://dev.armaforces.com/";
            var apiService = new ApiModsetClient(baseUrl);
            IModset modset = apiService.GetModsetDataByName("default-test").ConvertForServer();
            IModsetConfig modsetConfig = new ModsetConfig(settings, modset.Name);
            modsetConfig.LoadConfig();
            var server = new Server(settings, modsetConfig);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}