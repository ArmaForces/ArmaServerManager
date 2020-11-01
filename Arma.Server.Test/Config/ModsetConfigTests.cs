using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Arma.Server.Config;
using Arma.Server.Test.Helpers;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Arma.Server.Test.Config
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
        }

        [Fact]
        public void ModsetConfig_LoadConfig_Success()
        {
            _settingsMock.Setup(settings => settings.ModsetConfigDirectoryName).Returns(_modsetConfigDirName);
            _configMock.Setup(x => x.DirectoryPath).Returns(_workingDirectory);
            var expectedModsetConfigDirectory = Path.Join(_workingDirectory, _modsetConfigDirName);
            var expectedModsetConfigFiles = new[] { "server.cfg", "basic.cfg", "config.json" };

            IModsetConfig modsetConfig = new ModsetConfig(
                _configMock.Object,
                _settingsMock.Object,
                _modsetName,
                _fileSystemMock);
            var configLoaded = modsetConfig.LoadConfig();

            using (new AssertionScope())
            {
                configLoaded.IsSuccess.Should().BeTrue();
                _fileSystemMock.Directory.Exists(expectedModsetConfigDirectory).Should().BeTrue();
                _fileSystemMock.Directory.GetFiles(expectedModsetConfigDirectory).Should().BeEquivalentTo(expectedModsetConfigFiles);
            }
        }
    }
}