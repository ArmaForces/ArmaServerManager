using System;

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
        public required DateTime DeleteFrom { get; init; }
        
        /// <summary>
        /// TODO:
        /// </summary>
        public required DateTime DeleteTo { get; init; }
    }
}