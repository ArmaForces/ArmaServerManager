using System;
using System.Collections.Generic;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public class WebModset {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<WebMod> Mods { get; set; }
    }
}