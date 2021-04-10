using System;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public class WebMod
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public WebModSource Source { get; set; }

        public WebModType Type { get; set; }

        public long ItemId { get; set; }

        public string? Directory { get; set; }

        public Mod ConvertForServer() 
            => new Mod
            {
                Name = Name,
                WebId = Id,
                WorkshopId = ItemId,
                Source = (ModSource)Source,
                Type = (ModType)Type,
                Directory = Directory
            };
    }
}