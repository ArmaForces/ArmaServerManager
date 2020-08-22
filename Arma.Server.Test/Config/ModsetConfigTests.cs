using System;
using System.IO;
using Arma.Server.Config;
using AutoFixture;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test.Config {
    public class ModlistConfigTests : IDisposable {
        private static readonly Fixture Fixture = new Fixture();
        private readonly string _modlistName = Fixture.Create<string>();
        private readonly string _serverConfigDirName = Fixture.Create<string>();
        private readonly string _modlistConfigDirName = Fixture.Create<string>();
        private readonly string _serverConfigDirPath;
        private readonly string _modlistConfigDirPath;

        public ModlistConfigTests() {
            _serverConfigDirPath = Path.Join(Directory.GetCurrentDirectory(), _serverConfigDirName);
            _modlistConfigDirPath = Path.Join(_serverConfigDirPath, _modlistConfigDirName, _modlistName);
        }

        public void Dispose() {
            Directory.Delete(_serverConfigDirPath, true);
        }

        [Fact]
        public void ModlistConfig_LoadConfig_Success() {
            // Arrange
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(settings => settings.ServerDirectory).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.ServerConfigDirectory)
                .Returns(_serverConfigDirPath);
            settingsMock.Setup(settings => settings.ModlistConfigDirectoryName)
                .Returns(_modlistConfigDirName);

            // Act
            IModlistConfig modlistConfig = new ModlistConfig(settingsMock.Object, _modlistName);
            var configLoaded = modlistConfig.LoadConfig();

            // Assert
            configLoaded.Should().BeEquivalentTo(Result.Success());
            Assert.True(Directory.Exists(_modlistConfigDirPath));
            Assert.True(File.Exists(Path.Join(_modlistConfigDirPath, "server.cfg")));
            Assert.True(File.Exists(Path.Join(_modlistConfigDirPath, "basic.cfg")));
            Assert.True(File.Exists(Path.Join(_modlistConfigDirPath, "config.json")));
        }
    }
}