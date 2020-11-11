using System;
using System.IO;
using Arma.Server.Config;
using Arma.Server.Modset;
using Arma.Server.Providers;
using Arma.Server.Providers.Parameters;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test {
    public class ServerTests : IDisposable {
        private readonly Mock<ISettings> _settingsMock;
        private readonly Mock<IParametersProvider> _parametersProviderMock;

        private readonly Fixture _fixture = new Fixture();
        private readonly Server _server;

        public ServerTests() {
            _settingsMock = new Mock<ISettings>();
            var _serverConfigDir = Path.Join(Directory.GetCurrentDirectory(), _fixture.Create<string>());
            _settingsMock.Setup(x => x.ServerDirectory).Returns(Directory.GetCurrentDirectory());
            _settingsMock.Setup(x => x.ServerConfigDirectory).Returns(_serverConfigDir);
            _settingsMock.Setup(x => x.ServerExecutable).Returns(Directory.GetCurrentDirectory());

            _parametersProviderMock = new Mock<IParametersProvider>();
            _parametersProviderMock.Setup(x => x.GetStartupParams()).Returns(string.Empty);

            // Create server
            _server = new Server(_settingsMock.Object, _parametersProviderMock.Object, null);
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