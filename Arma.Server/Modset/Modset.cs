using System;
using System.Collections.Generic;

namespace Arma.Server.Modset {
    public class Modset {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<Mod.Mod> Mods { get; set; }
    }
}