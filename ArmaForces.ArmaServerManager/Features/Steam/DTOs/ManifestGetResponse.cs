using BytexDigital.Steam.ContentDelivery.Models;

namespace ArmaForces.ArmaServerManager.Features.Steam.DTOs
{
    public class ManifestGetResponse
    {
        public uint ItemId { get; set; }

        public Manifest? Manifest { get; set; }
    }
}
