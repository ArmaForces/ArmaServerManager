using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal interface IManifestDownloader
    {
        Task<Manifest> GetManifest(ContentItem contentItem, CancellationToken cancellationToken);
    }
}
