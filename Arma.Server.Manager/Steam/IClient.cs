using System.Threading.Tasks;

namespace Arma.Server.Manager.Steam {
    public interface IClient {
        IDownloader Downloader { get; }
        Task Connect();
        void Disconnect();
    }
}