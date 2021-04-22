using System;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    public class JobScheduleRequestDto
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? ScheduleAt { get; set; }
    }
}
