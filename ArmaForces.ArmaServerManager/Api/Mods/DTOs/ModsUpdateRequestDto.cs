using System;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;
using Newtonsoft.Json;

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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? ModsetName { get; set; }

        /// <summary>
        /// Ids of mods to update.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<long>? Mods { get; set; }
    }
}
