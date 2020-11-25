using System;

namespace Arma.Server.Manager.Features.Server.DTOs
{
    public class ModsUpdateRequest
    {
        public string ModsetName { get; set; }

        public DateTime? ScheduleAt { get; set; }
    }
}
