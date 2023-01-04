using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;

namespace ArmaForces.ArmaServerManager.Api.Servers
{
    internal static class ServerStatusMapper
    {
        public static ServerStatusDto Map(ServerStatus serverStatus)
            => new ServerStatusDto
            {
                Status = serverStatus.Status,
                Name = serverStatus.Name,
                Map = serverStatus.Map,
                ModsetName = serverStatus.ModsetName,
                Port = serverStatus.Port,
                Players = serverStatus.Players,
                PlayersMax = serverStatus.PlayersMax,
                StartTime = serverStatus.StartTime,
                HeadlessClientsConnected = serverStatus.HeadlessClientsConnected
            };
    }
}