using System.Collections.Generic;
using System.Threading.Tasks;
using Arma.Server.Config;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core;
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

        /// <inheritdoc />
        public async Task Connect() {
            await _bytexSteamClient.ConnectAsync();
        }

        /// <inheritdoc />
        public void Disconnect() {
            _bytexSteamClient.Shutdown();
        }

        /// <inheritdoc />
        public async Task Download(int itemId)
            => await Downloader.DownloadMod(itemId);

        /// <inheritdoc />
        public async Task Download(IEnumerable<int> itemsIds)
            => await Downloader.DownloadMods(itemsIds);
    }
}