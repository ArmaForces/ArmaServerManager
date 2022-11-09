using System;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// TODO:
    /// </summary>
    public record JobsDeleteRequestDto
    {
        /// <summary>
        /// TODO:
        /// </summary>
        [JsonProperty("deleteFrom", Required = Required.Always)]
        public DateTime? DeleteFrom { get; init; }
        
        /// <summary>
        /// TODO:
        /// </summary>
        [JsonProperty("deleteTo", Required = Required.Always)]
        public DateTime? DeleteTo { get; init; }
    }
}