using System.Threading;
using System.Threading.Tasks;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;

namespace Arma.Server.Manager.Features.Steam.Content
{
    public class ContentDownloadHandler : IContentDownloadHandler
    {
        private readonly MultipleFilesHandler _multipleFilesHandler;
        private readonly IDownloadHandler _downloadHandler;

        public ContentDownloadHandler(IDownloadHandler downloadHandler)
        {
            if (downloadHandler is MultipleFilesHandler multipleFilesHandler)
            {
                _multipleFilesHandler = multipleFilesHandler;
            }
            else
            {
                _downloadHandler = downloadHandler;
            }
        }

        public double TotalProgress => _multipleFilesHandler?.TotalProgress ?? _downloadHandler.TotalProgress;

        public async Task DownloadChangesToFolderAsync(string directory, CancellationToken cancellationToken) 
            => await (_multipleFilesHandler?.DownloadChangesToFolderAsync(directory, cancellationToken) ??
                      _downloadHandler.DownloadToFolderAsync(directory, cancellationToken));
    }
}
