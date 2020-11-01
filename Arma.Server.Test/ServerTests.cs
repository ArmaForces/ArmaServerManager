using System;
using System.IO;
using Arma.Server.Config;
using Arma.Server.Modset;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test {
    public class ServerTests : IDisposable {
        private readonly Mock<ISettings> _settingsMock;
        private readonly Mock<IModsetConfig> _modsetConfigMock;
        private readonly Mock<IModset> _modsetMock;

        private readonly Fixture _fixture = new Fixture();
        private readonly Server _server;

        public ServerTests() {
            _settingsMock = new Mock<ISettings>();
            var _serverConfigDir = Path.Join(Directory.GetCurrentDirectory(), _fixture.Create<string>());
            _settingsMock.Setup(x => x.ServerDirectory).Returns(Directory.GetCurrentDirectory());
            _settingsMock.Setup(x => x.ServerConfigDirectory).Returns(_serverConfigDir);
            _settingsMock.Setup(x => x.ServerExecutable).Returns(Directory.GetCurrentDirectory());
            _modsetMock = new Mock<IModset>();
            _modsetMock.Setup(x => x.Name).Returns(_fixture.Create<string>());
            _modsetConfigMock = new Mock<IModsetConfig>();

            // Create server
            _server = new Server(_settingsMock.Object, _modsetConfigMock.Object, _modsetMock.Object);
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