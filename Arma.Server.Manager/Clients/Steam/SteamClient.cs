using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core;
using Microsoft.Extensions.DependencyInjection;
using BytexSteamClient = BytexDigital.Steam.Core.SteamClient;

namespace Arma.Server.Manager.Clients.Steam {
    /// <inheritdoc />
    public class SteamClient : ISteamClient {
        private const int SteamAppId = 233780; // Arma 3 Server
        private readonly SteamCredentials _steamCredentials;

        private readonly BytexSteamClient _bytexSteamClient;
        public SteamContentClient ContentClient { get; }

        /// <inheritdoc />
        /// <param name="settings">Settings containing steam user, password and mods directory.</param>
        public SteamClient(ISettings settings) : this(settings.SteamUser, settings.SteamPassword, settings) { }

        /// <inheritdoc cref="SteamClient" />
        /// <param name="user">Steam username.</param>
        /// <param name="password">Steam password.</param>
        /// <param name="settings">Settings containing mods directory.</param>
        public SteamClient(string user, string password, ISettings settings) {
            _steamCredentials = new SteamCredentials(user, password);
            _bytexSteamClient = new BytexSteamClient(_steamCredentials);
            ContentClient = new SteamContentClient(_bytexSteamClient);
        }

        public static SteamClient CreateSteamClient(IServiceProvider serviceProvider)
        {
            return new SteamClient(serviceProvider.GetService<ISettings>());
        }

        /// <inheritdoc />
        /// <exception cref="OperationCanceledException">Thrown when <see cref="CancellationToken"/> is cancelled.</exception>
        /// <exception cref="InvalidCredentialException">Thrown when Steam credentials are invalid and connection could not be established.</exception>
        public async Task EnsureConnected(CancellationToken cancellationToken) {
            var connectCancellationTokenSource = new CancellationTokenSource();
            var connectTask = _bytexSteamClient.ConnectAsync(connectCancellationTokenSource.Token);
            var connectionTimeout = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.WhenAny(connectTask, connectionTimeout);

            if (cancellationToken.IsCancellationRequested) {
                connectCancellationTokenSource.Cancel();
                throw new OperationCanceledException(cancellationToken);
            }

            if (connectTask.Status == TaskStatus.WaitingForActivation) {
                connectCancellationTokenSource.Cancel();
                throw new InvalidCredentialException("Invalid Steam Credentials");
            }
        }

        /// <inheritdoc />
        public void Disconnect() {
            _bytexSteamClient.Shutdown();
        }
    }
}