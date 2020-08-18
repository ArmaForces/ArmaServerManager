using Arma.Server.Config;

namespace Arma.Server.Manager {
    internal class Program {
        static void Main(string[] args) {
            var _settings = new Settings();
            var modlist = new Modlist.Modlist();
            var modlistConfig = new ModlistConfig(_settings, modlist.GetName());
            modlistConfig.LoadConfig();
            var server = new Server(_settings, modlistConfig);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}