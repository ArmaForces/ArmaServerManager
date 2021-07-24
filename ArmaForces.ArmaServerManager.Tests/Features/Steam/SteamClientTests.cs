using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.ArmaServerManager.Features.Steam;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Steam
{
    [Trait("Category", "Integration")]
    public class SteamClientTests
    {
        [Fact]
        public void Connect_CancellationRequested_TaskCancelled()
        {
            using var steamClient = CreateSteamClient();
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            // ReSharper disable once AccessToDisposedClosure - steamClient should not be disposed here
            Func<Task> action = async () => await steamClient.EnsureConnected(cancellationTokenSource.Token);

            action.Should().Throw<OperationCanceledException>();
        }

        [Fact]
        public void Connect_InvalidCredentials_ThrowsInvalidCredentialsException()
        {
            using var steamClient = CreateSteamClient();

            // ReSharper disable once AccessToDisposedClosure - steamClient should not be disposed here
            Func<Task> action = async () => await steamClient.EnsureConnected(CancellationToken.None);

            action.Should().Throw<InvalidCredentialException>("Invalid Steam Credentials");
        }

        private static SteamClient CreateSteamClient()
            => new SteamClient(PrepareMockedSettings(), NullLogger<SteamClient>.Instance);

        private static ISettings PrepareMockedSettings()
        {
            var settingsMock = new Mock<ISettings>();
            
            settingsMock.Setup(x => x.SteamUser).Returns(string.Empty);
            settingsMock.Setup(x => x.SteamPassword).Returns(string.Empty);
            settingsMock.Setup(x => x.ModsDirectory).Returns(string.Empty);
            
            return settingsMock.Object;
        }
    }
}
