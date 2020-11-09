using System;
using System.Collections.Generic;
using System.Linq;
using Arma.Server.Mod;

namespace Arma.Server.Manager.Clients.Modsets.Entities {
    public class WebModset {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<WebMod> Mods { get; set; }

        public Arma.Server.Modset.Modset ConvertForServer() 
            => new Arma.Server.Modset.Modset
            {
                Name = Name,
                WebId = Id,
                Mods = Mods.Select(x => (IMod) x.ConvertForServer()).ToHashSet()
            };
    }
}