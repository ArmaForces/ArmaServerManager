using System;
using System.ComponentModel.DataAnnotations;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    /// <summary>
    /// Server status details.
    /// </summary>
    public class ServerStatusDto
    {
        /// <summary>
        /// Current server status.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [SwaggerSchema(Nullable = false)]
        [Required]
        public ServerStatusEnum Status { get; set; }

        /// <summary>
        /// Server name visible in server browser.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        /// <summary>
        /// Name of the modset currently loaded on the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? ModsetName { get; set; }

        /// <summary>
        /// Name of the map currently loaded on the server. Note that it's not pretty name.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? Map { get; set; }

        /// <summary>
        /// Current number of players connected to the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Players { get; set; }

        /// <summary>
        /// Maximum number of players allowed to connect to the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PlayersMax { get; set; }

        /// <summary>
        /// Server port for connections.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Port {get; set; }
        
        /// <summary>
        /// Time when server was first started with current settings.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Number of headless clients connected to the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? HeadlessClientsConnected { get; set; }
    }
}