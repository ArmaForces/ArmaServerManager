using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core;
using BytexSteamClient = BytexDigital.Steam.Core.SteamClient;

namespace ArmaForces.ArmaServerManager.Features.Steam
{
    /// <inheritdoc cref="ISteamClient" />
    public class SteamClient : ISteamClient, IDisposable
    {
        private readonly BytexSteamClient _bytexSteamClient;

        /// <inheritdoc />
        /// <param name="settings">Settings containing steam user, password and mods directory.</param>
        /// TODO: Handle no Steam User/Password
        public SteamClient(ISettings settings) : this(settings.SteamUser!, settings.SteamPassword!)
        {
        }

        /// <inheritdoc cref="SteamClient" />
        /// <param name="user">Steam username.</param>
        /// <param name="password">Steam password.</param>
        public SteamClient(string user, string password)
        {
            var steamCredentials = new SteamCredentials(user, password);
            _bytexSteamClient = new BytexSteamClient(steamCredentials);
            ContentClient = new SteamContentClient(_bytexSteamClient);
        }

        public SteamContentClient ContentClient { get; }

        /// <inheritdoc />
        /// <exception cref="OperationCanceledException">Thrown when <see cref="CancellationToken" /> is cancelled.</exception>
        /// <exception cref="InvalidCredentialException">
        ///     Thrown when Steam credentials are invalid and connection could not be
        ///     established.
        /// </exception>
        public async Task EnsureConnected(CancellationToken cancellationToken)
        {
            var connectCancellationTokenSource = new CancellationTokenSource();
            var connectTask = _bytexSteamClient.ConnectAsync(connectCancellationTokenSource.Token);
            var connectionTimeout = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.WhenAny(connectTask, connectionTimeout);

            if (cancellationToken.IsCancellationRequested)
            {
                connectCancellationTokenSource.Cancel();
                throw new OperationCanceledException(cancellationToken);
            }

            if (connectTask.Status == TaskStatus.WaitingForActivation)
            {
                connectCancellationTokenSource.Cancel();
                throw new InvalidCredentialException("Invalid Steam Credentials");
            }
        }

        // TODO: Consider 'using' when operating on SteamClient, probably limit it to job scope 
        public void Dispose() => Disconnect();

        /// <inheritdoc />
        public void Disconnect() => _bytexSteamClient.Shutdown();
    }
}
