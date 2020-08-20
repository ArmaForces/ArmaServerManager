using System;
using System.Collections.Generic;
using Arma.Server.Mod;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public class Modlist : IModlist {
        public string GetId() => Id;
        public string GetName() => Name;
        public List<Mod.Mod> GetModsList() => Mods;

        [JsonProperty]
        public string Id { get; protected set; }

        [JsonProperty]
        public string Name { get; protected set; }

        [JsonProperty]
        public DateTime CreatedAt { get; protected set; }

        [JsonProperty]
        public DateTime? LastUpdatedAt { get; protected set; }

        [JsonProperty]
        public List<Mod.Mod> Mods { get; protected set; }
    }
}