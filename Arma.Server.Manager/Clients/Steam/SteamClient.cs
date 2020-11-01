using System;
using System.Collections.Generic;
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
        private readonly SteamContentClient _contentClient;
        private IDownloader Downloader { get; }

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
            _contentClient = new SteamContentClient(_bytexSteamClient);
            Downloader = new Downloader(this, _contentClient, settings.ModsDirectory);
        }

        public static SteamClient CreateSteamClient(IServiceProvider serviceProvider)
        {
            return new SteamClient(serviceProvider.GetService<ISettings>());
        }

        /// <inheritdoc />
        /// <exception cref="OperationCanceledException">Thrown when <see cref="CancellationToken"/> is cancelled.</exception>
        /// <exception cref="InvalidCredentialException">Thrown when Steam credentials are invalid and connection could not be established.</exception>
        public async Task Connect(CancellationToken cancellationToken) {
            var connectCancellationTokenSource = new CancellationTokenSource();
            var connectTask = _bytexSteamClient.ConnectAsync(connectCancellationTokenSource.Token);
            var connectionTimeout = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.WhenAny(connectTask, connectionTimeout);

            if (cancellationToken.IsCancellationRequested) {
                connectCancellationTokenSource.Cancel();
                Disconnect();
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

        /// <inheritdoc />
        public async Task Download(int itemId, CancellationToken cancellationToken) 
            => await Downloader.DownloadMod(itemId, cancellationToken);

        /// <inheritdoc />
        public async Task Download(IEnumerable<int> itemsIds, CancellationToken cancellationToken) 
            => await Downloader.DownloadMods(itemsIds, cancellationToken);
    }
}