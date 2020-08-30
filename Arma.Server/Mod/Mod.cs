using System;

namespace Arma.Server.Mod {
    public class Mod : IMod {
        public string WebId { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public ModSource Source { get; set; }

        public ModType Type { get; set; }

        public int WorkshopId { get; set; }

        public string Directory { get; set; }
    }
}