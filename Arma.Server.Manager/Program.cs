using Arma.Server.Config;
using Arma.Server.Modlist;

namespace Arma.Server.Manager {
    internal class Program {
        static void Main(string[] args) {
            var _settings = new Settings();
            var baseUrl = "https://dev.armaforces.com/";
            var apiService = new ApiModlistDataService(baseUrl);
            var modlist = apiService.GetModlistDataByName("default-test");
            var modlistConfig = new ModlistConfig(_settings, modlist.GetName());
            modlistConfig.LoadConfig();
            var server = new Server(_settings, modlistConfig);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}