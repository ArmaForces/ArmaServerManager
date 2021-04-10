using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.Arma.Server.Providers.Keys;
using ArmaForces.Arma.Server.Tests.Helpers;
using AutoFixture;
using CSharpFunctionalExtensions;
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

        private readonly Mock<IModsetConfig> _modsetConfigMock = new Mock<IModsetConfig>();
        private readonly Mock<IKeysProvider> _keysProviderMock = new Mock<IKeysProvider>();
        private readonly Mock<IArmaProcessManager> _armaProcessManagerMock = new Mock<IArmaProcessManager>();
        private readonly IModset _modset;

        private readonly Fixture _fixture = new Fixture();

        public DedicatedServerTests()
        {
            _modset = ModsetHelpers.CreateTestModset(_fixture);

            _modsetConfigMock
                .Setup(x => x.CopyConfigFiles())
                .Returns(Result.Success);
        }

        [Fact]
        public async Task IsServerStarted_NoServerProcessCreated_ReturnsServerStopped()
        {
            var dedicatedServer = PrepareDedicatedServer();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var serverStatus = await dedicatedServer.GetServerStatusAsync(cancellationTokenSource.Token);

            using (new AssertionScope())
            {
                serverStatus.Status.Should().Be(ServerStatusEnum.Stopped);
                dedicatedServer.IsServerStopped.Should().BeTrue();
            }
        }

        [Fact]
        public void Dispose_ServerDisposed_OnServerShutdownInvoked()
        {
            var dedicatedServer = PrepareDedicatedServer();

            var funcMock = new Mock<Func<IDedicatedServer, Task>>();
            dedicatedServer.OnServerShutdown += funcMock.Object;

            dedicatedServer.Dispose();

            funcMock.Verify(x => x.Invoke(It.IsAny<IDedicatedServer>()), Times.Once);
        }

        [Fact]
        public void Start_ServerIsStopped_ServerStarted()
        {
            var armaProcessMock = new Mock<IArmaProcess>();
            armaProcessMock
                .Setup(x => x.Start())
                .Returns(Result.Success);
            armaProcessMock
                .Setup(x => x.IsStopped)
                .Returns(true);

            var headlessClientsMocks = new List<Mock<IArmaProcess>>
            {
                new Mock<IArmaProcess>(),
                new Mock<IArmaProcess>()
            };
            foreach (var headlessClientMock in headlessClientsMocks)
            {
                headlessClientMock
                    .Setup(x => x.Start())
                    .Returns(Result.Success);
            }

            var dedicatedServer = PrepareDedicatedServer(
                armaProcessMock.Object,
                headlessClientsMocks.Select(x => x.Object));

            var result = dedicatedServer.Start();

            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();
                _modsetConfigMock.Verify(x => x.CopyConfigFiles(), Times.Once);
                _keysProviderMock.Verify(x => x.PrepareKeysForModset(_modset), Times.Once);
                armaProcessMock.Verify(x => x.Start(), Times.Once);
                foreach (var headlessClientMock in headlessClientsMocks)
                {
                    headlessClientMock.Verify(x => x.Start(), Times.Once);
                }
            }
        }

        [Fact]
        public void Start_ServerIsRunning_ThrowsServerRunningException()
        {
            var armaProcessMock = new Mock<IArmaProcess>();
            armaProcessMock
                .Setup(x => x.Start())
                .Returns(Result.Success);
            armaProcessMock
                .Setup(x => x.IsStopped)
                .Returns(false);
            
            var dedicatedServer = PrepareDedicatedServer(
                armaProcessMock.Object,
                new List<IArmaProcess>());

            Action startServerAction = () => dedicatedServer.Start();

            startServerAction.Should()
                .Throw<ServerRunningException>()
                .WithMessage("Cannot start a running server.");
        }

        [Fact]
        public async Task Shutdown_ServerShutdown_OnServerShutdownInvoked()
        {
            var dedicatedServer = PrepareDedicatedServer();

            var funcMock = new Mock<Func<IDedicatedServer, Task>>();
            dedicatedServer.OnServerShutdown += funcMock.Object;

            await dedicatedServer.Shutdown();

            funcMock.Verify(x => x.Invoke(It.IsAny<IDedicatedServer>()), Times.Once);
        }

        private DedicatedServer PrepareDedicatedServer()
        {
            var serverProcessMock = new Mock<IArmaProcess>();
            serverProcessMock.Setup(x => x.IsStopped).Returns(true);
            serverProcessMock.Setup(x => x.IsStartingOrStarted).Returns(false);
            return PrepareDedicatedServer(serverProcessMock.Object);
        }

        private DedicatedServer PrepareDedicatedServer(IArmaProcess armaProcess, IEnumerable<IArmaProcess>? headlessClients = null)
            => new DedicatedServer(
                ServerPort,
                _modset,
                _modsetConfigMock.Object,
                _keysProviderMock.Object,
                _armaProcessManagerMock.Object,
                armaProcess,
                headlessClients ?? new List<IArmaProcess>(),
                new Logger<DedicatedServer>(new NullLoggerFactory()));
    }
}
