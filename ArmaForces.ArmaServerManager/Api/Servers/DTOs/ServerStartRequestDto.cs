using System;
using System.ComponentModel.DataAnnotations;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    /// <summary>
    /// Request for starting the server.
    /// </summary>
    public class ServerStartRequestDto : ServerRestartRequestDto
    {
        /// <summary>
        /// Name of the modset which will be used to start the server.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        [SwaggerSchema(Nullable = false)]
        [Required]
        public string ModsetName { get; set; } = string.Empty;

        /// <summary>
        /// Port on which the server should be started. Use {port}/start endpoint.
        /// </summary>
        [Obsolete("Use {port}/start endpoint")]
        [JsonProperty(Required = Required.DisallowNull)]
        public int Port { get; set; } = 2302;
    }
}
