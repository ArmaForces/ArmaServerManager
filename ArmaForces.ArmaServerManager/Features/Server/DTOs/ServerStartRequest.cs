using System;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Features.Server.DTOs
{
    public class ServerStartRequest
    {
        [JsonProperty(Required = Required.Always)]
        public string ModsetName { get; set; } = string.Empty;

        public DateTime? ScheduleAt { get; set; }

        public int Port { get; set; } = 2302;
    }
}
