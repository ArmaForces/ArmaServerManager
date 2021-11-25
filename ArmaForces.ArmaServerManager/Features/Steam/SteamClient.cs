using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core;
using Microsoft.Extensions.Logging;
using BytexSteamClient = BytexDigital.Steam.Core.SteamClient;

namespace ArmaForces.ArmaServerManager.Features.Steam
{
    /// <inheritdoc cref="ISteamClient" />
    public class SteamClient : ISteamClient, IDisposable
    {
        private readonly ILogger<SteamClient> _logger;
        private readonly BytexSteamClient _bytexSteamClient;

        /// <inheritdoc cref="ISteamClient" />
        /// <param name="settings">Settings containing steam user, password and mods directory.</param>
        /// <param name="logger">Logger</param>
        /// TODO: Handle no Steam User/Password
        public SteamClient(
            ISettings settings,
            ILogger<SteamClient> logger)
        {
            var steamCredentials = new SteamCredentials(settings.SteamUser, settings.SteamPassword);
            _bytexSteamClient = new BytexSteamClient(steamCredentials);
            ContentClient = new SteamContentClient(_bytexSteamClient);
            _logger = logger;
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
            _logger.LogTrace("Ensuring connected to Steam");
            var connectCancellationTokenSource = new CancellationTokenSource();
            var connectTask = _bytexSteamClient.ConnectAsync(connectCancellationTokenSource.Token);
            var connectionTimeout = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.WhenAny(connectTask, connectionTimeout);

            if (cancellationToken.IsCancellationRequested)
            {
                connectCancellationTokenSource.Cancel();
                _logger.LogError("Could not ensure connection to Steam in 10 seconds");
                throw new OperationCanceledException(cancellationToken);
            }

            if (connectTask.Status == TaskStatus.WaitingForActivation)
            {
                connectCancellationTokenSource.Cancel();
                _logger.LogError("Invalid Steam credentials");
                throw new InvalidCredentialException("Invalid Steam Credentials");
            }
        }

        // TODO: Consider 'using' when operating on SteamClient, probably limit it to job scope 
        public void Dispose() => Disconnect();

        /// <inheritdoc />
        public void Disconnect()
        {
            _logger.LogTrace("Disconnecting from Steam");
            _bytexSteamClient.Shutdown();
        }
    }
}
