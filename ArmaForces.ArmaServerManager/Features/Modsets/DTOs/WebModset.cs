using System;
using System.Collections.Generic;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public class WebModset
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<WebMod> Mods { get; set; } = new List<WebMod>();
    }
}