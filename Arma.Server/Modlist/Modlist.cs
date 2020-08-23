using System;
using System.Collections.Generic;
using Arma.Server.Mod;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public class Modlist : IModlist {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public List<Mod.Mod> Mods { get; set; }
    }
}