using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Tests.Helpers;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Config
{
    public class ModsetConfigTests
    {
        private readonly string _workingDirectory = Directory.GetCurrentDirectory();

        private static readonly Fixture Fixture = new Fixture();

        private readonly string _modsetName = Fixture.Create<string>();
        private readonly string _modsetConfigDirName = Fixture.Create<string>();
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly Mock<IConfig> _configMock = new Mock<IConfig>();
        private readonly IFileSystem _fileSystemMock;

        public ModsetConfigTests()
        {
            _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>(), _workingDirectory);
            _fileSystemMock.Directory.CreateDirectory(_workingDirectory);
            MockedFileSystemHelpers.CopyExampleFilesToMockedFileSystem(_fileSystemMock, _workingDirectory);
            MockedFileSystemHelpers.CopyTestFilesToMockedFileSystem(_fileSystemMock, _workingDirectory);
        }

        [Fact]
        public void ModsetConfig_LoadConfig_Success()
        {
            _settingsMock.Setup(settings => settings.ModsetConfigDirectoryName).Returns(_modsetConfigDirName);
            _configMock.Setup(x => x.DirectoryPath).Returns(_workingDirectory);
            _configMock.Setup(x => x.BasicCfg).Returns(Path.Join(_workingDirectory, "example_basic.cfg"));
            _configMock.Setup(x => x.ServerCfg).Returns(Path.Join(_workingDirectory, "example_server.cfg"));
            _configMock.Setup(x => x.ConfigJson).Returns(Path.Join(_workingDirectory, "common.json"));

            var expectedModsetConfigDirectory = Path.Join(_workingDirectory, _modsetConfigDirName, _modsetName);
            var expectedModsetConfigFiles = new[] { "server.cfg", "basic.cfg", "config.json" };

            var configFileCreator = new ConfigFileCreator(new ConfigReplacer(new NullLogger<ConfigReplacer>()));

            IModsetConfig modsetConfig = new ModsetConfig(
                _configMock.Object,
                _settingsMock.Object,
                _modsetName,
                configFileCreator,
                new NullLogger<ModsetConfig>(),
                _fileSystemMock);
            var configLoaded = modsetConfig.LoadConfig();

            using (new AssertionScope())
            {
                configLoaded.IsSuccess.Should().BeTrue();
                _fileSystemMock.Directory.Exists(expectedModsetConfigDirectory).Should().BeTrue();
                _fileSystemMock.Directory.GetFiles(expectedModsetConfigDirectory)
                    .Select(Path.GetFileName).Should().BeEquivalentTo(expectedModsetConfigFiles);
            }
        }
    }
}