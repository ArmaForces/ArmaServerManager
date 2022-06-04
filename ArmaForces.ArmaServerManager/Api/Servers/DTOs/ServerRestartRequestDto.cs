using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    /// <summary>
    /// Request for server restart.
    /// Server will be restarted with the same settings as when this job is started.
    /// </summary>
    public class ServerRestartRequestDto : JobScheduleRequestDto
    {
        /// <summary>
        /// Should restart of the server running at the job startup be forced.
        /// When restart is not forced, old server will not be shut down until all players leave the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ForceRestart { get; set; }
    }
}
