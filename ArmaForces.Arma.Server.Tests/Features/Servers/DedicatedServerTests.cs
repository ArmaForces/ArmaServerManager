using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Servers
{
    public class DedicatedServerTests
    {
        private const int ServerPort = 2302;

        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly Mock<IModset> _modsetMock = new Mock<IModset>();
        private readonly Mock<IModsetConfig> _modsetConfigMock = new Mock<IModsetConfig>();
        private readonly Mock<IKeysProvider> _keysProviderMock = new Mock<IKeysProvider>();
        private readonly Mock<IArmaProcessManager> _armaProcessManagerMock = new Mock<IArmaProcessManager>();

        private readonly Mock<IServerConfigurationProvider> _serverConfigurationProvider =
            new Mock<IServerConfigurationProvider>();

        private readonly Fixture _fixture = new Fixture();

        public DedicatedServerTests()
        {
            var serverConfigDir = Path.Join(Directory.GetCurrentDirectory(), _fixture.Create<string>());
            _settingsMock.Setup(x => x.ServerDirectory).Returns(Directory.GetCurrentDirectory());
            _settingsMock.Setup(x => x.ServerConfigDirectory).Returns(serverConfigDir);
            _settingsMock.Setup(x => x.ServerExecutable).Returns(Directory.GetCurrentDirectory());

            var modsetName = _fixture.Create<string>();
            _modsetMock.Setup(x => x.Name).Returns(modsetName);

            _serverConfigurationProvider.Setup(x => x.GetModsetConfig(modsetName)).Returns(_modsetConfigMock.Object);
        }

        [Fact]
        public async Task IsServerStarted_NoServerProcessCreated_ReturnsServerStopped()
        {
            var dedicatedServer = PrepareDedicatedServer();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var serverStatus = await dedicatedServer.GetServerStatusAsync(cancellationTokenSource.Token);

            using (new AssertionScope())
            {
                serverStatus.IsServerRunning.Should().BeFalse();
                serverStatus.IsServerStarting.Should().BeFalse();
                dedicatedServer.IsServerStopped.Should().BeTrue();
            }
        }

        [Fact]
        public void Dispose_DedicatedServerDisposed_OnServerShutdownInvoked()
        {
            var dedicatedServer = PrepareDedicatedServer();

            var funcMock = new Mock<Func<IDedicatedServer, Task>>();
            dedicatedServer.OnServerShutdown += funcMock.Object;

            dedicatedServer.Dispose();

            funcMock.Verify(x => x.Invoke(It.IsAny<IDedicatedServer>()), Times.Once);
        }

        private DedicatedServer PrepareDedicatedServer()
        {
            var serverProcessMock = new Mock<IArmaProcess>();
            serverProcessMock.Setup(x => x.IsStopped).Returns(true);
            serverProcessMock.Setup(x => x.IsStartingOrStarted).Returns(false);
            return PrepareDedicatedServer(serverProcessMock.Object);
        }

        private DedicatedServer PrepareDedicatedServer(IArmaProcess armaProcess, IEnumerable<IArmaProcess> headlessClients = null)
            => new DedicatedServer(
                ServerPort,
                _modsetMock.Object,
                _modsetConfigMock.Object,
                _keysProviderMock.Object,
                _armaProcessManagerMock.Object,
                armaProcess,
                headlessClients ?? new List<IArmaProcess>(),
                new Logger<DedicatedServer>(new NullLoggerFactory()));
    }
}
