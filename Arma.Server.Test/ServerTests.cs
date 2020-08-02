using System;
using System.IO;
using Arma.Server.Config;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test {
    public class ServerTests : IDisposable {
        private readonly Mock<ISettings> settingsMock;
        private readonly Mock<ModsetConfig> modsetConfigMock;
        private readonly Fixture _fixture = new Fixture();
        private readonly Server server;

        public ServerTests() {
            settingsMock = new Mock<ISettings>();
            settingsMock.Setup(x => x.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(x => x.GetSettingsValue("serverConfigDirName")).Returns(_fixture.Create<string>());
            settingsMock.Setup(x => x.GetServerExePath()).Returns(Directory.GetCurrentDirectory());
            modsetConfigMock = new Mock<ModsetConfig>(settingsMock.Object, _fixture.Create<string>());

            // Create server
            server = new Server(settingsMock.Object, modsetConfigMock.Object);
        }

        [Fact]
        public void Server_IsRunningBeforeStart_False() {
            // Assert
            server.IsServerRunning().Should().BeFalse();
        }

        /* These are more like integration tests and cannot be supported properly yet
        [Fact]
        public void Server_IsRunningAfterStart_True() {
            // Act
            server.Start();
            
            // Assert
            server.IsServerRunning().Should().BeTrue();
        }

        [Fact]
        public void Server_IsRunningAfterShutdown_False() {
            // Act
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();

            // Assert
            server.IsServerRunning().Should().BeFalse();
        }*/

        public void Dispose() {
            server.Shutdown();
        }
    }
}