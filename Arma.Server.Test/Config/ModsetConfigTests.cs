using System;
using System.IO;
using Arma.Server.Config;
using AutoFixture;
using Moq;
using Xunit;

namespace Arma.Server.Test.Config {
    public class ModsetConfigTests : IDisposable {
        private static readonly Fixture Fixture = new Fixture();
        private readonly string _modsetName = Fixture.Create<string>();
        private readonly string _serverConfigDirName = Fixture.Create<string>();
        private readonly string _serverConfigDirPath;
        private readonly string _modsetConfigDirPath;

        public ModsetConfigTests() {
            _serverConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), _serverConfigDirName);
            _modsetConfigDirPath = Path.Join(_serverConfigDirPath, "modsetConfigs", _modsetName);
        }

        public void Dispose() {
            Directory.Delete(_serverConfigDirPath, true);
        }

        [Fact]
        public void ModsetConfig_LoadConfig_Success() {
            // Arrange
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.GetSettingsValue("serverConfigDirName"))
                .Returns(_serverConfigDirName);

            // Act
            var modsetConfig = new ModsetConfig(settingsMock.Object, _modsetName);
            var configLoaded = modsetConfig.LoadConfig();

            // Assert
            Assert.True(configLoaded.IsSuccess);
            Assert.True(Directory.Exists(_modsetConfigDirPath));
            Assert.True(File.Exists(Path.Join(_modsetConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(_modsetConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(_modsetConfigDirPath, "config.json")));
        }
    }
}