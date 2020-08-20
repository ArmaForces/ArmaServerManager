using System;
using Newtonsoft.Json;

namespace Arma.Server.Mod {
    public class Mod : IMod {
        public string GetName() => Name;

        [JsonProperty]
        public string Id { get; protected set; }

        [JsonProperty]
        public string Name { get; protected set; }

        [JsonProperty]
        public DateTime CreatedAt { get; protected set; }

        [JsonProperty]
        public DateTime? LastUpdatedAt { get; protected set; }

        [JsonProperty]
        public ModSource Source { get; protected set; }

        [JsonProperty]
        public ModType Type { get; protected set; }

        [JsonProperty]
        public int ItemId { get; protected set; }

        [JsonProperty]
        public string Directory { get; protected set; }
    }
}