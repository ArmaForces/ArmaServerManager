using Arma.Server.Config;
using AutoFixture;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Arma.Server.Test.Config {
    public class SettingsTests {
        private const string DefaultServerExecutable = "arma3server_x64.exe";
        private Mock<IConfigurationRoot> _configurationMock = new Mock<IConfigurationRoot>();
        private Fixture _fixture = new Fixture();
        private MockFileSystem _fileSystemMock = new MockFileSystem();
        private readonly string _workingDirectory;

        public SettingsTests() {
            _workingDirectory = Path.Join(Directory.GetCurrentDirectory(), _fixture.Create<string>());
            _configurationMock.Setup(x => x["serverDirectory"]).Returns(_workingDirectory);
            _fileSystemMock.AddDirectory(_workingDirectory);
        }
        
        [Fact]
        public void Settings_LoadSettings_Success() {
            ISettings settings = new Settings(_configurationMock.Object, _fileSystemMock);

            var loaded = settings.LoadSettings();

            loaded.Should().BeEquivalentTo(Result.Success());
        }

        [Fact]
        public void Settings_ModsDirectoryCustom_Correct() {
            var expectedModsDirectory = Path.Join(_workingDirectory, _fixture.Create<string>());
            _configurationMock.Setup(x => x["modsDirectory"]).Returns(expectedModsDirectory);

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ModsDirectory.Should().BeEquivalentTo(expectedModsDirectory);
        }

        [Fact]
        public void Settings_ModsDirectoryDefault_Correct() {
            var expectedModsDirectory = Path.Join(_workingDirectory, "mods");

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ModsDirectory.Should().BeEquivalentTo(expectedModsDirectory);
        }

        [Fact]
        public void Settings_ServerConfigDirectoryCustom_Correct() {
            var expectedServerConfigDirectory = Path.Join(_workingDirectory, _fixture.Create<string>());
            _configurationMock.Setup(x => x["serverConfigDirectory"]).Returns(expectedServerConfigDirectory);

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ServerConfigDirectory.Should().BeEquivalentTo(expectedServerConfigDirectory);
        }

        [Fact]
        public void Settings_ServerConfigDirectoryDefault_Correct() {
            var expectedServerConfigDirectory = Path.Join(_workingDirectory, "serverConfig");

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ServerConfigDirectory.Should().BeEquivalentTo(expectedServerConfigDirectory);
        }

        [Fact]
        public void Settings_ServerDirectoryFromConfig_Correct() {
            var expectedServerDirectory = _workingDirectory;
            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ServerDirectory.Should().BeEquivalentTo(expectedServerDirectory);
        }

        [Fact]
        public void Settings_ServerDirectoryFromRegistry_Correct() {
            _configurationMock.Setup(x => x["serverDirectory"]).Returns(() => null);
            var expectedServerDirectory = _workingDirectory;
            var registryReaderMock = new Mock<IRegistryReader>();
            registryReaderMock.Setup(
                x => x.GetValueFromLocalMachine(@"SOFTWARE\WOW6432Node\bohemia interactive\arma 3", "main"))
                .Returns(expectedServerDirectory);

            var settings = PrepareSettings(_configurationMock, _fileSystemMock, registryReaderMock);

            settings.ServerDirectory.Should().BeEquivalentTo(expectedServerDirectory);
        }

        [Fact]
        public void Settings_ServerDirectoryNoCorrect_ThrowsServerNotFound() {
            _configurationMock.Setup(x => x["serverDirectory"]).Returns(() => null);
            var registryReaderMock = new Mock<IRegistryReader>();

            Action action = () => PrepareSettings(_configurationMock, _fileSystemMock, registryReaderMock);

            action.Should().Throw<ServerNotFoundException>("Could not find server directory.");
        }

        [Fact]
        public void Settings_ServerDirectory_FoundOrThrewException() {
            try {
                var settings = PrepareSettings(_configurationMock, _fileSystemMock);

                settings.ServerDirectory.Should().NotBeNullOrEmpty();
            } catch (ServerNotFoundException e) {
                Assert.Contains(@"Server path could not be loaded", e.Message);
            }
        }
        
        [Fact]
        public void Settings_ServerExecutableDefault_Correct() {
            var expectedServerExecutable = Path.Join(_workingDirectory, DefaultServerExecutable);

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ServerExecutable.Should().BeEquivalentTo(expectedServerExecutable);
        }

        [Fact]
        public void Settings_ServerExecutableCustom_Correct() {
            var serverExecutableName = _fixture.Create<string>();
            var expectedServerExecutable = Path.Join(_workingDirectory, serverExecutableName);
            _configurationMock.Setup(x => x["serverExecutableName"]).Returns(serverExecutableName);
            _fileSystemMock.AddFile(expectedServerExecutable, "");

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ServerExecutable.Should().BeEquivalentTo(expectedServerExecutable);
        }

        [Fact]
        public void Settings_ServerExecutableCustomIncorrect_FallsBackToDefault() {
            var serverExecutableName = _fixture.Create<string>();
            var expectedServerExecutable = Path.Join(_workingDirectory, DefaultServerExecutable);
            _configurationMock.Setup(x => x["serverExecutableName"]).Returns(serverExecutableName);

            var settings = PrepareSettings(_configurationMock, _fileSystemMock);

            settings.ServerExecutable.Should().BeEquivalentTo(expectedServerExecutable);
        }

        private ISettings PrepareSettings(
            Mock<IConfigurationRoot> configurationMock,
            IFileSystem fileSystemMock,
            Mock<IRegistryReader> registryReaderMock = null) {
            ISettings settings = registryReaderMock is null
                ? new Settings(configurationMock.Object, fileSystemMock)
                : new Settings(configurationMock.Object, fileSystemMock, registryReaderMock.Object);
            settings.LoadSettings();

            return settings;
        }
    }
}