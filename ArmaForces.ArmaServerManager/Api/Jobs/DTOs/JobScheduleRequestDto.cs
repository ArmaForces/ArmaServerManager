using System;
using System.Text.Json.Serialization;

namespace ArmaForces.ArmaServerManager.Api.Jobs.DTOs
{
    /// <summary>
    /// Details for jobs scheduling.
    /// </summary>
    public class JobScheduleRequestDto
    {
        /// <summary>
        /// Time when job should be processed.
        /// Exact start time will depend on other jobs processing and enqueued at this time.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime? ScheduleAt { get; set; }
    }
}
