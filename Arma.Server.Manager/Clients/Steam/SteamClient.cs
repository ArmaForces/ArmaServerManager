using System.Collections.Generic;
using System.Threading.Tasks;
using Arma.Server.Config;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core;

namespace Arma.Server.Manager.Clients.Steam {
    public class SteamClient : ISteamClient {
        private const int SteamAppId = 233780; // Arma 3 Server
        private readonly SteamCredentials _steamCredentials;

        private readonly BytexDigital.Steam.Core.SteamClient _steamClient;
        private readonly SteamContentClient _contentClient;
        private IDownloader Downloader { get; }

        public SteamClient(ISettings settings) : this(settings.SteamUser, settings.SteamPassword, settings) { }

        public SteamClient(string user, string password, ISettings settings) {
            _steamCredentials = new SteamCredentials(user, password);
            _steamClient = new BytexDigital.Steam.Core.SteamClient(_steamCredentials);
            _contentClient = new SteamContentClient(_steamClient);
            Downloader = new Downloader(this, _contentClient, settings.ModsDirectory);
        }

        public async Task Connect() {
            await _steamClient.ConnectAsync();
        }

        public void Disconnect() {
            _steamClient.Shutdown();
        }

        //    await Task.WhenAll(itemsId.Select(Download).ToArray());

        public async Task Download(int itemId)
            => await Downloader.DownloadMod(itemId);

        public async Task Download(IEnumerable<int> itemsIds)
            => await Downloader.DownloadMods(itemsIds);
    }
}