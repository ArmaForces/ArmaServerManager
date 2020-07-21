using System.IO;
using ArmaServerManager;
using Moq;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigTests {
        [Fact]
        public void ServerConfig_LoadConfig_Success() {
            var modsetName = new Modset().GetName();
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName")).Returns("TestDir");
            
            var serverConfig = new ServerConfig(settingsMock.Object, modsetName);
            var configLoaded = serverConfig.LoadConfig();
            
            //todo add asserts for directory structure
            Assert.True(configLoaded.IsSuccess);
        }
    }
}