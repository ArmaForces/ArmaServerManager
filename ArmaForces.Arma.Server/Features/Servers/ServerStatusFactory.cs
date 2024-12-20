﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Servers
{
    internal class ServerStatusFactory : IServerStatusFactory
    {
        private readonly ILogger<ServerStatusFactory> _logger;

        public ServerStatusFactory(ILogger<ServerStatusFactory> logger)
        {
            _logger = logger;
        }

        public async Task<ServerStatus> GetServerStatus(IDedicatedServer dedicatedServer, CancellationToken cancellationToken)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Loopback, dedicatedServer.SteamQueryPort);
            var serverInfo = await A2SInfo.GetServerInfoAsync(ipEndPoint, cancellationToken, _logger);

            return new ServerStatus
            {
                Status = GetServerStatusEnum(dedicatedServer, serverInfo),
                Name = serverInfo?.Name,
                ModsetName = dedicatedServer.Modset.Name,
                Map = serverInfo?.Map,
                Players = serverInfo?.Players,
                PlayersMax = serverInfo?.MaxPlayers,
                Port = dedicatedServer?.Port,
                StartTime = dedicatedServer?.StartTime,
                HeadlessClientsConnected = dedicatedServer?.HeadlessClientsConnected
            };
        }

        private static ServerStatusEnum GetServerStatusEnum(IDedicatedServer? dedicatedServer, A2SInfo? serverInfo)
        {
            if (!(serverInfo is null))
            {
                return ServerStatusEnum.Started;
            }

            if (!dedicatedServer?.IsServerStopped ?? false)
            {
                return ServerStatusEnum.Starting;
            }

            return ServerStatusEnum.Stopped;
        }
    }
}
