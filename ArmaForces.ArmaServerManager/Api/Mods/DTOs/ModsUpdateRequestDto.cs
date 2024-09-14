using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;

namespace ArmaForces.ArmaServerManager.Api.Mods.DTOs
{
    /// <summary>
    /// Request to update mods.
    /// </summary>
    public class ModsUpdateRequestDto : JobScheduleRequestDto
    {
        /// <summary>
        /// Name of the modset to update.
        /// Use {modsetName}/update to update mods from a modset.
        /// </summary>
        [Obsolete("Use {modsetName}/update for updating modset.")]
        public string? ModsetName { get; set; }
        
        /// <summary>
        /// Ids of mods to update.
        /// </summary>
        public List<long>? Mods { get; set; }
    }
}
