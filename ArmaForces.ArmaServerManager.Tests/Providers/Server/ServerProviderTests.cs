using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.ArmaServerManager.Features.Servers.Providers;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using FluentAssertions.Execution;
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
        private const int HeadlessClientsCount = 4;
        
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
        public void GetOrAddServer_CalledTwoTimes_ServerShouldBeTheSame()
        {
            var dedicatedServerFactoryMock = CreateDedicatedServerFactoryMock(ServerPort);

            var serverProvider = PrepareServerProvider();

            var firstDedicatedServer = serverProvider.GetOrAddServer(ServerPort, ServerFactoryFunc(dedicatedServerFactoryMock));
            var secondDedicatedServer = serverProvider.GetOrAddServer(ServerPort, ServerFactoryFunc(dedicatedServerFactoryMock));

            using (new AssertionScope())
            {
                dedicatedServerFactoryMock
                    .Verify(x => x.CreateDedicatedServer(ServerPort, _modset, HeadlessClientsCount), Times.Once);
                
                firstDedicatedServer.Should().Be(secondDedicatedServer);
            }
        }

        [Fact]
        public void GetOrAddServer_CalledTwoTimesFirstServerDisposed_NewServerReturned()
        {
            var dedicatedServerFactoryMock = CreateDedicatedServerFactoryMock(ServerPort);

            var serverProvider = PrepareServerProvider();

            var firstDedicatedServer = serverProvider.GetOrAddServer(ServerPort, ServerFactoryFunc(dedicatedServerFactoryMock));
            serverProvider.TryRemoveServer(firstDedicatedServer);
            var secondDedicatedServer = serverProvider.GetOrAddServer(ServerPort, ServerFactoryFunc(dedicatedServerFactoryMock));

            using (new AssertionScope())
            {
                dedicatedServerFactoryMock
                    .Verify(x => x.CreateDedicatedServer(ServerPort, _modset, HeadlessClientsCount), Times.Exactly(2));
                
                firstDedicatedServer.Should().NotBe(secondDedicatedServer);
            }
        }

        private Mock<IDedicatedServerFactory> CreateDedicatedServerFactoryMock(int serverPort)
        {
            var dedicatedServerFactoryMock = new Mock<IDedicatedServerFactory>();
            
            dedicatedServerFactoryMock
                .Setup(
                    x => x.CreateDedicatedServer(
                        serverPort,
                        _modset,
                        HeadlessClientsCount))
                .Returns(() => new TestDedicatedServer(_modset)
                {
                    HeadlessClientsConnected = HeadlessClientsCount
                    
                });
            
            return dedicatedServerFactoryMock;
        }

        private ServerProvider PrepareServerProvider(Dictionary<int, IDedicatedServer>? servers = null)
        {
            return new ServerProvider(servers ?? new Dictionary<int, IDedicatedServer>(), _logger);
        }

        private Func<int, IDedicatedServer> ServerFactoryFunc(Mock<IDedicatedServerFactory> dedicatedServerFactoryMock)
            => port => dedicatedServerFactoryMock.Object.CreateDedicatedServer(port, _modset, HeadlessClientsCount);
    }
}
