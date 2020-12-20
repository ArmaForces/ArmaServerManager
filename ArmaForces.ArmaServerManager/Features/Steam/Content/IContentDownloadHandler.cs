using System.Threading;
using System.Threading.Tasks;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    public interface IContentDownloadHandler
    {
        double TotalProgress { get; }

        Task DownloadChangesToFolderAsync(string directory, CancellationToken cancellationToken);
    }
}
