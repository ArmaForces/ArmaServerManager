using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Mods.DTOs
{
    /// <summary>
    /// Request for mods verification.
    /// </summary>
    public class ModsVerificationRequestDto : JobScheduleRequestDto
    {
        /// <summary>
        /// Ids of mods to verify.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<long>? Mods { get; set; }
    }
}
