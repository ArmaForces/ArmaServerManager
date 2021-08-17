using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using ArmaForces.ArmaServerManager.Features.Steam.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Controller
{
    [Route("api/steam")]
    [ApiController]
    public class SteamController : ControllerBase
    {
        private readonly IManifestDownloader _manifestDownloader;
        
        public SteamController(IManifestDownloader manifestDownloader)
        {
            _manifestDownloader = manifestDownloader;
        }

        [HttpGet("manifest")]
        public async Task<IActionResult> GetManifest([FromQuery] List<uint> itemIds)
        {
            var manifests = await GetManifests(itemIds);
            
            return Ok(manifests);
        }

        private async Task<List<ManifestGetResponse>> GetManifests(IReadOnlyCollection<uint> itemIds)
        {
            return await itemIds.ToAsyncEnumerable()
                .Select(x => new ContentItem {Id = x})
                .SelectAwait(async x => await DownloadManifest(x))
                .Select(x => new ManifestGetResponse
                {
                    ItemId = x.ItemId,
                    Manifest = x.Manifest
                })
                .ToListAsync();
        }

        private async Task<(uint ItemId, Manifest? Manifest)> DownloadManifest(ContentItem contentItem)
        {
            try
            {
                return (contentItem.Id, await _manifestDownloader.GetManifest(contentItem, CancellationToken.None));
            }
            catch (Exception)
            {
                return (contentItem.Id, null);
            }
        }
    }
}
