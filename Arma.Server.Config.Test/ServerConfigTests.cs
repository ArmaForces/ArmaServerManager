using System;
using System.IO;
using AutoFixture;
using Moq;
using Xunit;

namespace Arma.Server.Config.Test {
    public class ServerConfigTests : IDisposable {
        private static readonly Fixture Fixture = new Fixture();
        private readonly string _serverConfigDirName = Fixture.Create<string>();
        private readonly string _serverConfigDirPath;

        public ServerConfigTests() {
            _serverConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), _serverConfigDirName);
        }

        public void Dispose() {
            Directory.Delete(_serverConfigDirPath, true);
        }

        [Fact]
        public void ServerConfig_LoadConfig_Success() {
            // Arrange
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName"))
                .Returns(_serverConfigDirName);

            // Act
            var serverConfig = new ServerConfig(settingsMock.Object);
            var configLoaded = serverConfig.LoadConfig();

            // Assert
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(_serverConfigDirPath));
            Assert.True(File.Exists(Path.Join(_serverConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(_serverConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(_serverConfigDirPath, "common.json")));
            Assert.True(File.Exists(Path.Join(_serverConfigDirPath, "common.Arma3Profile")));
        }
    }
}