using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arma.Server.Config;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core;
using BytexDigital.Steam.Core.Enumerations;

namespace Arma.Server.Manager.Steam {
    public class Client : IClient {
        private const int SteamAppId = 233780; // Arma 3 Server
        private protected readonly SteamCredentials _steamCredentials;

        private readonly SteamClient _steamClient;
        private readonly SteamContentClient _contentClient;
        private IDownloader Downloader { get; }

        public Client(ISettings settings) : this(settings.SteamUser, settings.SteamPassword, settings) { }

        public Client(string user, string password, ISettings settings) {
            _steamCredentials = new SteamCredentials(user, password);
            _steamClient = new SteamClient(_steamCredentials);
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