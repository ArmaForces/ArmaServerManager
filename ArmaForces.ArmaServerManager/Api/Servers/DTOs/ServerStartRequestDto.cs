using System;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    public class ServerStartRequestDto : JobScheduleRequestDto
    {
        [JsonProperty(Required = Required.Always)]
        public string ModsetName { get; set; } = string.Empty;

        [Obsolete]
        [JsonProperty(Required = Required.DisallowNull)]
        public int Port { get; set; } = 2302;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ForceRestart { get; set; }
    }
}
