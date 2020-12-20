using System;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public class WebMod {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public WebModSource Source { get; set; }

        public WebModType Type { get; set; }

        public int ItemId { get; set; }

        public string Directory { get; set; }

        public Mod ConvertForServer() 
            => new Mod
            {
                Name = Name,
                WebId = Id,
                WorkshopId = ItemId,
                Source = (ModSource)Source,
                Type = (ModType)Type
            };
    }
}