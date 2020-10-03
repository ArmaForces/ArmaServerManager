using System;

namespace Arma.Server.Manager.Clients.Modsets.Entities {
    public class WebMod {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public WebModSource Source { get; set; }

        public WebModType Type { get; set; }

        public int ItemId { get; set; }

        public string Directory { get; set; }

        public Mod.Mod ConvertForServer() 
            => new Mod.Mod
            {
                Name = Name,
                WebId = Id,
                WorkshopId = ItemId,
                Source = (Mod.ModSource)Source,
                Type = (Mod.ModType)Type
            };
    }
}