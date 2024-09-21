using System;
using System.ComponentModel.DataAnnotations;
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
        public int HeadlessClients { get; set; } = DefaultHeadlessClients;
        
        /// <summary>
        /// Name of the modset which will be used to start the server.
        /// </summary>
        [SwaggerSchema(Nullable = false)]
        [Required]
        public required string ModsetName { get; set; } = string.Empty;

        /// <summary>
        /// Port on which the server should be started. Use {port}/start endpoint.
        /// </summary>
        [Obsolete("Use {port}/start endpoint")]
        public int Port { get; set; } = 2302;
    }
}
