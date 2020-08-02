using Arma.Server.Config;

namespace Arma.Server.Manager {
    internal class Program {
        static void Main(string[] args) {
            var _settings = new Settings();
            var modset = new Modset();
            var modsetConfig = new ModsetConfig(_settings, modset.GetName());
            modsetConfig.LoadConfig();
            var server = new Server(_settings, modsetConfig);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
        }
    }
}