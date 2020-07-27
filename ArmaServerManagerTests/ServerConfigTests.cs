using System.IO;
using ArmaServerManager;
using Moq;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigTests {
        private const string ModsetName = "default";
        private const string ServerConfigDirName = "TestDir";

        [Fact]
        public void ServerConfig_LoadConfig_Success() {
            var serverConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), ServerConfigDirName);
            var modsetConfigDirPath = Path.Join(ServerConfigDirName, "modsetConfigs", "default");
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName")).Returns(ServerConfigDirName);
            
            var serverConfig = new ServerConfig(settingsMock.Object);
            var configLoaded = serverConfig.LoadConfig();
            
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(serverConfigDirPath));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "common.json")));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "common.Arma3Profile")));

            // Clear
            Directory.Delete(serverConfigDirPath, true);
        }
    }
}