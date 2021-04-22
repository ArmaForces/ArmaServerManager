using ArmaForces.ArmaServerManager.Api.Jobs.DTOs;

namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    public class ServerRestartRequestDto : JobScheduleRequestDto
    {
        public bool? ForceRestart { get; set; }
    }
}
