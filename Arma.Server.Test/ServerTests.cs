using System;
using System.IO;
using Arma.Server.Config;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test {
    public class ServerTests : IDisposable {
        private readonly Mock<ISettings> _settingsMock;
        private readonly Mock<ModlistConfig> _modlistConfigMock;
        private readonly Fixture _fixture = new Fixture();
        private readonly Server _server;

        public ServerTests() {
            _settingsMock = new Mock<ISettings>();
            _settingsMock.Setup(x => x.ServerExecutable).Returns(Directory.GetCurrentDirectory());
            _settingsMock.Setup(x => x.ServerConfigDirectoryName).Returns(_fixture.Create<string>());
            _settingsMock.Setup(x => x.ServerExecutable).Returns(Directory.GetCurrentDirectory());
            _modlistConfigMock = new Mock<ModlistConfig>(_settingsMock.Object, _fixture.Create<string>());

            // Create server
            _server = new Server(_settingsMock.Object, _modlistConfigMock.Object);
        }

        [Fact]
        public void Server_IsRunningBeforeStart_False() {
            // Assert
            _server.IsServerRunning().Should().BeFalse();
        }

        public void Dispose() {
            _server.Shutdown();
        }
    }
}