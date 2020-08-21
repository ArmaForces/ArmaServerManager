using System;
using Newtonsoft.Json;

namespace Arma.Server.Mod {
    public class Mod : IMod {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public DateTime CreatedAt { get; set; }

        [JsonProperty]
        public DateTime? LastUpdatedAt { get; set; }

        [JsonProperty]
        public ModSource Source { get; set; }

        [JsonProperty]
        public ModType Type { get; set; }

        [JsonProperty]
        public int ItemId { get; set; }

        [JsonProperty]
        public string Directory { get; set; }
    }
}