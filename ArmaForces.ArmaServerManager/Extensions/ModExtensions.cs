using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using CSharpFunctionalExtensions;

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

        /// <summary>
        /// Updates <paramref name="olderMod"/> with relevant data from <paramref name="newerMod"/>.
        /// </summary>
        /// <param name="olderMod">Mod to be updated.</param>
        /// <param name="newerMod">Mod used as source for updated data.</param>
        /// <returns>New <see cref="IMod"/> with updated data.</returns>
        public static Result<IMod> UpdateModData(this IMod olderMod, IMod newerMod)
        {
            var mod = new Mod
            {
                Directory = newerMod.Directory ?? olderMod.Directory,
                CreatedAt = olderMod.CreatedAt,
                LastUpdatedAt = newerMod.LastUpdatedAt ?? olderMod.LastUpdatedAt,
                ManifestId = newerMod.ManifestId ?? olderMod.ManifestId,
                Name = newerMod.Name,
                Source = newerMod.Source,
                Type = newerMod.Type,
                WebId = newerMod.WebId ?? olderMod.WebId,
                WorkshopId = newerMod.WorkshopId
            };

            return Result.Success<IMod>(mod);
        }
    }
}
