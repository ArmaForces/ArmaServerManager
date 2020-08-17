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
        private readonly Mock<ModsetConfig> _modsetConfigMock;
        private readonly Fixture _fixture = new Fixture();
        private readonly Server _server;

        public ServerTests() {
            _settingsMock = new Mock<ISettings>();
            _settingsMock.Setup(x => x.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            _settingsMock.Setup(x => x.GetSettingsValue("serverConfigDirName")).Returns(_fixture.Create<string>());
            _settingsMock.Setup(x => x.GetServerExePath()).Returns(Directory.GetCurrentDirectory());
            _modsetConfigMock = new Mock<ModsetConfig>(_settingsMock.Object, _fixture.Create<string>());

            // Create server
            _server = new Server(_settingsMock.Object, _modsetConfigMock.Object);
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