using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arma.Server.Modlist {
    public class Modlist : IModlist {
        public string GetName() => Name;

        [JsonProperty]
        public string Id { get; protected set; }

        [JsonProperty]
        public string Name { get; protected set; }

        [JsonProperty]
        public DateTime CreatedAt { get; protected set; }

        [JsonProperty]
        public DateTime? LastUpdatedAt { get; protected set; }
    }
}