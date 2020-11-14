using System;
using Arma.Server.Config;
using Arma.Server.Features.Server;
using Arma.Server.Manager.Providers.Server;
using Arma.Server.Providers.Configuration;
using Arma.Server.Test.Helpers;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Arma.Server.Manager.Test.Providers.Server
{
    public class ServerProviderTests
    {
        [Fact]
        public void GetServer_CalledTwoTimes_ServerShouldBeTheSame()
        {
            const int serverPort = 2302;
            var testServiceProvider = PrepareTestServiceProvider();
            var modset = ModsetHelpers.CreateEmptyModset(new Fixture());

            var serverProvider = new ServerProvider(testServiceProvider);

            var firstDedicatedServer = serverProvider.GetServer(serverPort, modset);
            var secondDedicatedServer = serverProvider.GetServer(serverPort, modset);

            firstDedicatedServer.Should().Be(secondDedicatedServer);
        }

        [Fact]
        public void GetServer_CalledTwoTimesFirstServerDisposed_NewServerReturned()
        {
            const int serverPort = 2302;
            var testServiceProvider = PrepareTestServiceProvider();
            var modset = ModsetHelpers.CreateEmptyModset(new Fixture());

            var serverProvider = new ServerProvider(testServiceProvider);

            var firstDedicatedServer = serverProvider.GetServer(serverPort, modset);
            firstDedicatedServer.Dispose();

            var secondDedicatedServer = serverProvider.GetServer(serverPort, modset);

            firstDedicatedServer.Should().NotBe(secondDedicatedServer);
        }

        private static IServiceProvider PrepareTestServiceProvider()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            var settingsMock = new Mock<ISettings>();
            var serverConfigurationProvider = new Mock<IServerConfigurationProvider>();
            var logger = new Logger<DedicatedServer>(new LoggerFactory());

            serviceProviderMock.Setup(x => x.GetService(typeof(ISettings)))
                .Returns(settingsMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IServerConfigurationProvider)))
                .Returns(serverConfigurationProvider.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<DedicatedServer>)))
                .Returns(logger);

            return serviceProviderMock.Object;
        }
    }
}
