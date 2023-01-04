using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    /// <summary>
    /// Request for starting the server.
    /// </summary>
    public class ServerStartRequestDto : ServerRestartRequestDto
    {
        // TODO: Remove that or something
        internal const int DefaultHeadlessClients = 1;
        
        /// <summary>
        /// Number of Headless Clients to start alongside the server.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public int HeadlessClients { get; set; } = DefaultHeadlessClients;
        
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
