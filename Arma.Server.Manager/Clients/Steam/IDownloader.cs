using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Clients.Steam {
    public interface IDownloader {
        Task DownloadArmaServer();
        Task DownloadMod(int itemId);
        Task DownloadMods(IEnumerable<int> itemsIds);
    }
}