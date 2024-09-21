using System;

namespace ArmaForces.Arma.Server.Features.Servers.DTOs
{
    public class ServerStatus(
        ServerStatusEnum status = ServerStatusEnum.Stopped,
        string? name = null,
        string? modsetName = null,
        string? mapName = null,
        int? playersCount = null,
        int? playersMaxCount = null,
        int? port = null,
        DateTimeOffset? startTime = null,
        int? headlessClientsCount = null)
    {
        public ServerStatusEnum Status { get; init; } = status;

        public string? Name { get; init; } = name;

        public string? ModsetName { get; init; } = modsetName;

        public string? Map { get; init;  } = mapName;

        public int? Players { get; init;  } = playersCount;

        public int? PlayersMax { get; init;  } = playersMaxCount;

        public int? Port {get; init;  } = port;

        public DateTimeOffset? StartTime { get; init; } = startTime;

        public int? HeadlessClientsConnected { get; init; } = headlessClientsCount;
    }
}
