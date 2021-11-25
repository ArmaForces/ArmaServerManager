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
    [Trait("Category", "Unit")]
    public class ServerConfigTests
    {
        private readonly string _workingDirectory = Directory.GetCurrentDirectory();
        private static readonly Fixture Fixture = new Fixture();
        private readonly string _serverConfigDirName = Fixture.Create<string>();
        private readonly IFileSystem _fileSystemMock;

        public ServerConfigTests()
        {
            _fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>(), _workingDirectory);
            _fileSystemMock.Directory.CreateDirectory(_workingDirectory);
            MockedFileSystemHelpers.CopyExampleFilesToMockedFileSystem(_fileSystemMock, _workingDirectory);
        }

        [Fact]
        public void ServerConfig_LoadConfig_Success()
        {
            // Arrange
            var settingsMock = new Mock<ISettings>();
            var serverConfigDirPath = Path.Join(_workingDirectory, _serverConfigDirName);
            settingsMock.Setup(settings => settings.ServerExecutable).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(settings => settings.ServerConfigDirectory)
                .Returns(serverConfigDirPath);
            var expectedServerConfigFiles = new[] { "server.cfg", "basic.cfg", "common.json", "common.Arma3Profile" };

            // Act
            IConfig serverConfig = new ServerConfig(settingsMock.Object, NullLogger<ServerConfig>.Instance, _fileSystemMock);
            var configLoaded = serverConfig.CopyConfigFiles();

            // Assert
            using (new AssertionScope())
            {
                configLoaded.IsSuccess.Should().BeTrue();
                _fileSystemMock.Directory.Exists(serverConfigDirPath).Should().BeTrue();
                _fileSystemMock.Directory.GetFiles(serverConfigDirPath)
                    .Select(Path.GetFileName).Should().BeEquivalentTo(expectedServerConfigFiles);
            }
        }
    }
}