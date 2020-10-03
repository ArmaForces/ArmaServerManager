using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BytexDigital.Steam.ContentDelivery;
using BytexDigital.Steam.Core.Enumerations;

namespace Arma.Server.Manager.Clients.Steam {
    public class Downloader: IDownloader {
        private const int SteamAppId = 233780; // Arma 3 Server
        private const int SteamDepotId = 228990;
        private readonly SteamContentClient _contentClient;
        private readonly ISteamClient _steamSteamClient;
        private readonly string _modsDirectory;

        public Downloader(ISteamClient steamSteamClient,
            SteamContentClient contentClient,
            string modsDirectory) {
            _steamSteamClient = steamSteamClient;
            _contentClient = contentClient;
            _modsDirectory = modsDirectory;
        }

        public async Task DownloadArmaServer()
            => await Download(itemType:ItemType.App);

        public async Task DownloadMods(IEnumerable<int> itemsIds) {
            await _steamSteamClient.Connect();
            foreach (int itemId in itemsIds)
            {
                await Download(itemId);
            }
            _steamSteamClient.Disconnect();
        }

        public async Task DownloadMod(int itemId)
            => await Download(itemId);

        private async Task Download(int itemId = 0)
            => await Download((uint) itemId);

        private async Task Download(uint itemId = 0, ItemType itemType = ItemType.Mod) {
            try {
                if (itemType == ItemType.App)
                    throw new NotImplementedException("Downloading Arma 3 Server is not supported yet.");

                var downloadHandler = itemType == ItemType.App
                    ? await _contentClient.GetAppDataAsync(SteamAppId, SteamDepotId, os: SteamOs.Windows)
                    : await _contentClient.GetPublishedFileDataAsync(itemId, os: SteamOs.Windows);

                Console.WriteLine($"Starting download of {itemId}");
                var downloadDirectory = Path.Join(_modsDirectory, itemId.ToString());
                var downloadTask = downloadHandler.DownloadToFolderAsync(downloadDirectory);

                while (!downloadTask.IsCompleted)
                {
                    var delayTask = Task.Delay(1000);
                    var t = await Task.WhenAny(delayTask, downloadTask);
                    Console.WriteLine($"Progress {(downloadHandler.TotalProgress * 100).ToString("00.00")}%");
                }

                await downloadTask;
                Console.WriteLine($"Downloaded {itemId}.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}