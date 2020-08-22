using Arma.Server.Config;
using Arma.Server.Modlist;

namespace Arma.Server.Manager {
    internal class Program {
        static void Main(string[] args) {
            ISettings settings = new Settings();
            var baseUrl = "https://dev.armaforces.com/";
            var apiService = new ApiModlistDataService(baseUrl);
            var modlist = apiService.GetModlistDataByName("default-test");
            IModlistConfig modlistConfig = new ModlistConfig(settings, modlist.Name);
            var server = new Server(settings, modlistConfig);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}