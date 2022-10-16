using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Servers
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServerQueryLogic
    {
        Result<IDedicatedServer> GetServer(int port);

        Task<ServerStatus> GetServerStatus(int port, CancellationToken cancellationToken);

        Task<List<ServerStatus>> GetAllServersStatus(CancellationToken cancellationToken);
    }
}