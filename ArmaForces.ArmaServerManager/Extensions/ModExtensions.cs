using System;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Extensions
{
    public static class ModExtensions
    {
        public static ContentItem AsContentItem(this Mod mod)
            => mod.Source == ModSource.SteamWorkshop
                ? new ContentItem
                {
                    Directory = mod.Directory,
                    Id = (uint) mod.WorkshopId!,
                    ItemType = ItemType.Mod
                }
                : throw new InvalidOperationException("Non Steam mods cannot be converted to content item.");

        /// <summary>
        /// Updates <paramref name="olderMod"/> with relevant data from <paramref name="newerMod"/>.
        /// </summary>
        /// <param name="olderMod">Mod to be updated.</param>
        /// <param name="newerMod">Mod used as source for updated data.</param>
        /// <returns>New <see cref="Mod"/> with updated data.</returns>
        public static Result<Mod> UpdateModData(this Mod olderMod, Mod newerMod)
        {
            var mod = new Mod
            {
                Directory = newerMod.Directory ?? olderMod.Directory,
                CreatedAt = olderMod.CreatedAt,
                LastUpdatedAt = newerMod.LastUpdatedAt ?? olderMod.LastUpdatedAt,
                Name = newerMod.Name,
                Source = newerMod.Source,
                Type = newerMod.Type,
                WebId = newerMod.WebId ?? olderMod.WebId,
                WorkshopId = newerMod.WorkshopId
            };

            return Result.Success<Mod>(mod);
        }
    }
}
