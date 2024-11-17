using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;

namespace ArmaForces.ArmaServerManager.Api.Mods.DTOs
{
    /// <summary>
    /// Request for mods verification.
    /// </summary>
    public class ModsVerificationRequestDto : JobScheduleRequestDto
    {
        /// <summary>
        /// Ids of mods to verify.
        /// </summary>
        public List<long>? Mods { get; set; }
    }
}
