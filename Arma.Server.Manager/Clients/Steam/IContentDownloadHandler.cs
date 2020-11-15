using System.Threading;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Clients.Steam
{
    public interface IContentDownloadHandler
    {
        double TotalProgress { get; }

        Task DownloadChangesToFolderAsync(string directory, CancellationToken cancellationToken);
    }
}
