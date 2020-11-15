using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Features.Mods
{
    public interface IModsDownloader
    {
        Task<List<Result>> DownloadOrUpdate(IEnumerable<int> modsIds, CancellationToken cancellationToken);
    }
}
