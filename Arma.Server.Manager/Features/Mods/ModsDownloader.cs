using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Clients.Steam;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Features.Mods
{
    public class ModsDownloader : IModsDownloader
    {
        private readonly IContentDownloader _contentDownloader;

        public ModsDownloader(IContentDownloader contentDownloader)
        {
            _contentDownloader = contentDownloader;
        }

        public static ModsDownloader CreateModsDownloader(IServiceProvider serviceProvider)
            => new ModsDownloader(serviceProvider.GetService<IContentDownloader>());

        public async Task<List<Result>> Download(IEnumerable<int> modsIds, CancellationToken cancellationToken) 
            => await _contentDownloader.Download(
                modsIds.Select(id => new KeyValuePair<int, ItemType>(id, ItemType.Mod)).ToList(),
                cancellationToken);

        public async Task<List<Result>> Update(IEnumerable<int> modsIds, CancellationToken cancellationToken) 
            => await _contentDownloader.Update(
                modsIds.Select(id => new KeyValuePair<int, ItemType>(id, ItemType.Mod)).ToList(),
                cancellationToken);
    }
}
