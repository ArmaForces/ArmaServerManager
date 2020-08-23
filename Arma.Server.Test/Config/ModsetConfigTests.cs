using System;
using System.IO;
using Arma.Server.Config;
using AutoFixture;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test.Config {
    public class ModsetConfigTests : IDisposable {
        private static readonly Fixture Fixture = new Fixture();
        private readonly string _modsetName = Fixture.Create<string>();
        private readonly string _serverConfigDirName = Fixture.Create<string>();
        private readonly string _modsetConfigDirName = Fixture.Create<string>();
        private readonly string _serverConfigDirPath;
        private readonly string _modsetConfigDirPath;

        public ModsetConfigTests() {
            _serverConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), _serverConfigDirName);
            _modsetConfigDirPath = Path.Join(_serverConfigDirPath, _modsetConfigDirName, _modsetName);
        }

        public void Dispose() {
            Directory.Delete(_serverConfigDirPath, true);
        }

        [Fact]
        public void ModsetConfig_LoadConfig_Success() {
            // Arrange
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.ServerDirectory).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.ServerConfigDirectory)
                .Returns(_serverConfigDirPath);
            settingsMock.Setup(settings => settings.ModsetConfigDirectoryName)
                .Returns(_modsetConfigDirName);

            // Act
            IModsetConfig modsetConfig = new ModsetConfig(settingsMock.Object, _modsetName);
            var configLoaded = modsetConfig.LoadConfig();

            // Assert
            configLoaded.IsSuccess.Should().BeTrue();
            Assert.True(Directory.Exists(_modsetConfigDirPath));
            Assert.True(File.Exists(Path.Join(_modsetConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(_modsetConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(_modsetConfigDirPath, "config.json")));
        }
    }
}