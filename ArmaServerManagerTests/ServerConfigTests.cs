using System.IO;
using ArmaServerManager;
using Moq;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigTests {
        [Fact]
        public void ServerConfig_LoadConfig_Success() {
            var modsetName = "default";
            var serverConfigDirName = "TestDir";
            var serverConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), serverConfigDirName);
            var modsetConfigDirPath = Path.Join(serverConfigDirName, "modsetConfigs", "default");
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName")).Returns(serverConfigDirName);
            
            var serverConfig = new ServerConfig(settingsMock.Object, modsetName);
            var configLoaded = serverConfig.LoadConfig();
            
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(serverConfigDirPath));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "common.json")));
            Assert.True(File.Exists(Path.Join(serverConfigDirPath, "common.Arma3Profile")));
            Assert.True(Directory.Exists(modsetConfigDirPath)); 
            Assert.True(File.Exists(Path.Join(modsetConfigDirPath, "config.json")));

            // Clear
            Directory.Delete(serverConfigDirPath, true);
        }
    }
}