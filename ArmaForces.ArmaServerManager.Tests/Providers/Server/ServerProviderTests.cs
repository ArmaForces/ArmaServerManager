using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.ArmaServerManager.Providers;
using ArmaForces.ArmaServerManager.Providers.Server;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Providers.Server
{
    [Trait("Category", "Unit")]
    public class ServerProviderTests
    {
        private readonly IModset _modset = ModsetHelpers.CreateEmptyModset(new Fixture());
        private readonly ILogger<ServerProvider> _logger = new NullLogger<ServerProvider>();

        [Fact]
        public void GetServer_CalledTwoTimes_ServerShouldBeTheSame()
        {
            const int serverPort = 2302;
            var modsetProviderMock = new Mock<IModsetProvider>();

            var dedicatedServerFactoryMock = new Mock<IDedicatedServerFactory>();
            dedicatedServerFactoryMock
                .Setup(x => x.CreateDedicatedServer(serverPort, _modset, 1))
                .Returns(() => new TestDedicatedServer(_modset));

            var serverProvider = new ServerProvider(
                modsetProviderMock.Object,
                PrepareProcessDiscoverer(),
                dedicatedServerFactoryMock.Object,
                _logger);

            var firstDedicatedServer = serverProvider.GetServer(serverPort, _modset);
            var secondDedicatedServer = serverProvider.GetServer(serverPort, _modset);

            dedicatedServerFactoryMock
                .Verify(x => x.CreateDedicatedServer(serverPort, _modset, 1), Times.Once);
            firstDedicatedServer.Should().Be(secondDedicatedServer);
        }

        [Fact]
        public void GetServer_CalledTwoTimesFirstServerDisposed_NewServerReturned()
        {
            const int serverPort = 2302;
            var modsetProviderMock = new Mock<IModsetProvider>();

            var dedicatedServerFactoryMock = new Mock<IDedicatedServerFactory>();
            dedicatedServerFactoryMock
                .Setup(x => x.CreateDedicatedServer(serverPort, _modset, 1))
                .Returns(() => new TestDedicatedServer(_modset));

            var serverProvider = new ServerProvider(
                modsetProviderMock.Object,
                PrepareProcessDiscoverer(),
                dedicatedServerFactoryMock.Object,
                _logger);

            var firstDedicatedServer = serverProvider.GetServer(serverPort, _modset);
            firstDedicatedServer.Dispose();

            var secondDedicatedServer = serverProvider.GetServer(serverPort, _modset);

            dedicatedServerFactoryMock
                .Verify(x => x.CreateDedicatedServer(serverPort, _modset, 1), Times.Exactly(2));
            firstDedicatedServer.Should().NotBe(secondDedicatedServer);
        }

        private static IArmaProcessDiscoverer PrepareProcessDiscoverer()
        {
            var armaProcessDiscovererMock = new Mock<IArmaProcessDiscoverer>();
            armaProcessDiscovererMock
                .Setup(x => x.DiscoverArmaProcesses())
                .Returns(new Task<Dictionary<int, List<IArmaProcess>>>(() => new Dictionary<int, List<IArmaProcess>>()));

            return armaProcessDiscovererMock.Object;
        }
    }
}
