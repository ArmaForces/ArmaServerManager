using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Server;
using ArmaForces.Arma.Server.Modset;
using ArmaForces.Arma.Server.Providers.Configuration;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Server
{
    public class DedicatedServerTests
    {
        private const int ServerPort = 2302;

        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly Mock<IModset> _modsetMock = new Mock<IModset>();
        private readonly Mock<IModsetConfig> _modsetConfigMock = new Mock<IModsetConfig>();

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
        public void Dispose_DedicatedServerDisposed_DisposedEventInvoked()
        {
            var dedicatedServer = PrepareDedicatedServer();

            using var monitoredServer = dedicatedServer.Monitor();
            dedicatedServer.Dispose();
            monitoredServer.Should().Raise("Disposed");
        }

        private DedicatedServer PrepareDedicatedServer()
            => new DedicatedServer(
                ServerPort,
                _settingsMock.Object,
                _modsetMock.Object,
                _serverConfigurationProvider.Object,
                new Logger<DedicatedServer>(new NullLoggerFactory()),
                new Logger<ServerProcess>(new NullLoggerFactory()));
    }
}
