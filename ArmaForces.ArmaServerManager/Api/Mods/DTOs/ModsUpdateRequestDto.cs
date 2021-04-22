using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Mods.DTOs
{
    public class ModsUpdateRequestDto : JobScheduleRequestDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? ModsetName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<IMod>? Mods { get; set; }
    }
}
