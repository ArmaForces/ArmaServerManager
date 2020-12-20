using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    /// <summary>
    ///     Handles downloading/updating mods and arma server.
    /// </summary>
    public interface IContentDownloader
    {
        Task<List<Result<IMod>>> DownloadOrUpdateMods(IEnumerable<IMod> mods, CancellationToken cancellationToken);
        
        Task<List<Result<ContentItem>>> DownloadOrUpdate(IEnumerable<ContentItem> items, CancellationToken cancellationToken);

        /// <summary>
        ///     Downloads <paramref name="contentItem"/>.
        /// </summary>
        /// <param name="contentItem">Item to download.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> for safe download abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        Task<Result<ContentItem>> DownloadOrUpdate(ContentItem contentItem, CancellationToken cancellationToken);
    }
}
