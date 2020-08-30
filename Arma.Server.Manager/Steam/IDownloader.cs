using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Steam {
    public interface IDownloader {
        Task DownloadArmaServer();
        Task DownloadMod(uint itemId);
        Task DownloadMods(IEnumerable<uint> itemsIds);
    }
}