using ArmaServerManager;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigTests {
        [Fact]
        public void ServerConfig_LoadConfig_Success() {
            var modsetName = new Modset().GetName();
            var settings = new Settings();
            var serverConfig = new ServerConfig(settings, modsetName);
            
            var configLoaded = serverConfig.LoadConfig();

            Assert.True(configLoaded.IsSuccess);
        }
    }
}