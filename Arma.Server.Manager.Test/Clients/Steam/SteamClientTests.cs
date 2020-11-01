using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Clients.Steam;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Manager.Test.Clients.Steam
{
    public class SteamClientTests
    {
        [Fact]
        public void Connect_CancellationRequested_TaskCancelled()
        {
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(x => x.SteamUser).Returns("");
            settingsMock.Setup(x => x.SteamPassword).Returns("");
            settingsMock.Setup(x => x.ModsDirectory).Returns("");
            var steamClient = new SteamClient(settingsMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            Func<Task> action = async () => await steamClient.EnsureConnected(cancellationTokenSource.Token);

            action.Should().Throw<OperationCanceledException>();
        }

        [Fact]
        public void Connect_InvalidCredentials_ThrowsInvalidCredentialsException()
        {
            var settingsMock = new Mock<ISettings>();
            settingsMock.Setup(x => x.SteamUser).Returns("");
            settingsMock.Setup(x => x.SteamPassword).Returns("");
            settingsMock.Setup(x => x.ModsDirectory).Returns("");
            var steamClient = new SteamClient(settingsMock.Object);

            Func<Task> action = async () => await steamClient.EnsureConnected(CancellationToken.None);

            action.Should().Throw<InvalidCredentialException>("Invalid Steam Credentials");
        }
    }
}
