using System;
using System.Collections.Generic;
using Arma.Server.Modset;

namespace Arma.Server.Manager.Modset {
    public class Modset : IModset {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<Mod.Mod> Mods { get; set; }
    }
}