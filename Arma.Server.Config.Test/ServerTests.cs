using System.IO;
using AutoFixture;
using Moq;
using Xunit;

namespace Arma.Server.Config.Test {
    public class ServerTests
    {

        private readonly Mock<ISettings> settingsMock;
        private readonly Mock<ModsetConfig> modsetConfigMock;
        private readonly Fixture _fixture = new Fixture();

        public ServerTests()
        {
            settingsMock = new Mock<ISettings>();
            settingsMock.Setup(x => x.GetServerPath()).Returns(Directory.GetCurrentDirectory());
            settingsMock.Setup(x => x.GetSettingsValue("serverConfigDirName")).Returns(_fixture.Create<string>());
            settingsMock.Setup(x => x.GetServerExePath()).Returns(Directory.GetCurrentDirectory());
            modsetConfigMock = new Mock<ModsetConfig>(settingsMock.Object, _fixture.Create<string>());
        }

        [Fact]
        public void Server_IsRunningBeforeStart_Success()
        {
            Manager.Server server = new Manager.Server(settingsMock.Object, modsetConfigMock.Object);
            Assert.False(server.IsServerRunning());
        }

        [Fact]
        public void Server_IsRunningAfterStart_Success()
        {
            Manager.Server server = new Manager.Server(settingsMock.Object, modsetConfigMock.Object);
            server.Start();
            Assert.True(server.IsServerRunning());
            server.Shutdown();
        }

        [Fact]
        public void Server_Shutdown_Success()
        {
            Manager.Server server = new Manager.Server(settingsMock.Object, modsetConfigMock.Object);
            server.Start();
            server.WaitUntilStarted();
            server.Shutdown();
            Assert.False(server.IsServerRunning());
        }
    }
}