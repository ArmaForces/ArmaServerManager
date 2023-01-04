using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Mods.DTOs
{
    /// <summary>
    /// Request to update mods.
    /// </summary>
    public class ModsetUpdateRequestDto : JobScheduleRequestDto
    {
        /// <summary>
        /// Should shutdown of all servers running at the job startup be forced.
        /// When it is not forced, old servers will not be shut down until all players leave servers.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Force { get; set; }
    }
}