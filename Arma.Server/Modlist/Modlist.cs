using System;
using System.Collections.Generic;
using Arma.Server.Mod;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public class Modlist : IModlist {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public DateTime CreatedAt { get; set; }

        [JsonProperty]
        public DateTime? LastUpdatedAt { get; set; }

        [JsonProperty]
        public List<Mod.Mod> Mods { get; set; }
    }
}