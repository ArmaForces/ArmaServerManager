using System;
using System.Collections.Generic;

namespace ArmaForces.ArmaServerManager.Clients.Modsets.Entities {
    public class WebModset {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<WebMod> Mods { get; set; }
    }
}