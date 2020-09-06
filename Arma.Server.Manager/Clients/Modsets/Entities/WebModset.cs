using System;
using System.Collections.Generic;
using System.Linq;

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
                Mods = (HashSet<Mod.Mod>)Mods.Select(x => x.ConvertForServer())
            };
    }
}