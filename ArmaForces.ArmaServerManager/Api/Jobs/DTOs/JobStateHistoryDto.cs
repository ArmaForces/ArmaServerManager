using System;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Job state history.
    /// </summary>
    public class JobStateHistoryDto
    {
        /// <summary>
        /// Job state name.
        /// </summary>
        public JobStatus StateName { get; set; }
        
        /// <summary>
        /// Time when given job state was entered.
        /// </summary>
        public DateTime EnteredAt { get; set; }
    }
}