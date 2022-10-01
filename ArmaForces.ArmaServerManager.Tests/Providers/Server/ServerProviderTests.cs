using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Servers;
using AutoFixture;
using AutoFixture.Kernel;
using CSharpFunctionalExtensions;
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
        private const int ServerPort = 2302;
        
        private readonly Fixture _fixture = new Fixture();
        
        private readonly IModset _modset = ModsetHelpers.CreateEmptyModset(new Fixture());
        private readonly ILogger<ServerProvider> _logger = new NullLogger<ServerProvider>();

        public ServerProviderTests()
        {
            // TODO: Remove customization when removing IMod interface
            _fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IMod),
                    typeof(Mod)));
        }
        
        [Fact]
        public void GetServer_CalledTwoTimes_ServerShouldBeTheSame()
        {
            var mockedModsetProvider = CreateMockedModsetProvider();

            var dedicatedServerFactoryMock = CreateDedicatedServerFactoryMock(ServerPort);

            var serverProvider = new ServerProvider(
                mockedModsetProvider,
                PrepareProcessDiscoverer(),
                dedicatedServerFactoryMock.Object,
                _logger);

            var firstDedicatedServer = serverProvider.GetServer(ServerPort, _modset);
            var secondDedicatedServer = serverProvider.GetServer(ServerPort, _modset);

            dedicatedServerFactoryMock
                .Verify(x => x.CreateDedicatedServer(ServerPort, _modset, 1), Times.Once);
            firstDedicatedServer.Should().Be(secondDedicatedServer);
        }

        [Fact]
        public void GetServer_CalledTwoTimesFirstServerDisposed_NewServerReturned()
        {
            var mockedModsetProvider = CreateMockedModsetProvider();
            var dedicatedServerFactoryMock = CreateDedicatedServerFactoryMock(ServerPort);

            var serverProvider = new ServerProvider(
                mockedModsetProvider,
                PrepareProcessDiscoverer(),
                dedicatedServerFactoryMock.Object,
                _logger);

            var firstDedicatedServer = serverProvider.GetServer(ServerPort, _modset);
            firstDedicatedServer.Dispose();

            var secondDedicatedServer = serverProvider.GetServer(ServerPort, _modset);

            dedicatedServerFactoryMock
                .Verify(x => x.CreateDedicatedServer(ServerPort, _modset, 1), Times.Exactly(2));
            firstDedicatedServer.Should().NotBe(secondDedicatedServer);
        }

        private IModsetProvider CreateMockedModsetProvider()
        {
            var modsetProviderMock = new Mock<IModsetProvider>();

            modsetProviderMock
                .Setup(x => x.GetModsetByName(It.IsAny<string>()))
                // TODO: Probably fix return
                .Returns(Task.FromResult(Result.Success(_fixture.Create<Modset>().As<IModset>())));
            
            return modsetProviderMock.Object;
        }

        private Mock<IDedicatedServerFactory> CreateDedicatedServerFactoryMock(int serverPort)
        {
            var dedicatedServerFactoryMock = new Mock<IDedicatedServerFactory>();
            
            dedicatedServerFactoryMock
                .Setup(
                    x => x.CreateDedicatedServer(
                        serverPort,
                        _modset,
                        1))
                .Returns(() => new TestDedicatedServer(_modset));
            
            return dedicatedServerFactoryMock;
        }

        private static IArmaProcessDiscoverer PrepareProcessDiscoverer()
        {
            var armaProcessDiscovererMock = new Mock<IArmaProcessDiscoverer>();
            
            armaProcessDiscovererMock
                .Setup(x => x.DiscoverArmaProcesses())
                .Returns(Task.FromResult(new Dictionary<int, List<IArmaProcess>>()));

            return armaProcessDiscovererMock.Object;
        }
    }
}
