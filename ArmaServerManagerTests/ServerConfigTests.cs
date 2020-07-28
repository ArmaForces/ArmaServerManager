using System;
using System.IO;
using ArmaServerManager;
using Moq;
using Xunit;

namespace ArmaServerManagerTests {
    public class ServerConfigTests: IDisposable {
        private const string ModsetName = "default";
        private const string ServerConfigDirName = "TestDir";
        private readonly string ServerConfigDirPath;

        public ServerConfigTests()
        {
            ServerConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), ServerConfigDirName);
        }

        public void Dispose()
        {
            Directory.Delete(ServerConfigDirPath, true);
        }

        [Fact]
        public void ServerConfig_LoadConfig_Success() {
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName")).Returns(ServerConfigDirName);
            
            var serverConfig = new ServerConfig(settingsMock.Object);
            var configLoaded = serverConfig.LoadConfig();
            
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(ServerConfigDirPath));
            Assert.True(File.Exists(Path.Join(ServerConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(ServerConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(ServerConfigDirPath, "common.json")));
            Assert.True(File.Exists(Path.Join(ServerConfigDirPath, "common.Arma3Profile")));
        }
    }
}