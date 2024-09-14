using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal interface IManifestDownloader
    {
        Task<Result<Manifest>> GetManifest(ContentItem contentItem, CancellationToken cancellationToken);
    }
}
