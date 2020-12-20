using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;

namespace ArmaForces.ArmaServerManager.Extensions
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
