using Arma.Server.Manager.Features.Steam.Content.DTOs;
using Arma.Server.Mod;

namespace Arma.Server.Manager.Extensions
{
    public static class ModExtensions
    {
        public static ContentItem AsContentItem(this IMod mod)
            => new ContentItem
            {
                Directory = mod.Directory,
                Id = (uint) mod.WorkshopId,
                ItemType = ItemType.Mod,
                ManifestId = mod.ManifestId
            };
    }
}
