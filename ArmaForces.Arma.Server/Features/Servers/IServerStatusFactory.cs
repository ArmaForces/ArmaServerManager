using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers.DTOs;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public interface IServerStatusFactory
    {
        Task<ServerStatus> GetServerStatus(IDedicatedServer dedicatedServer, CancellationToken cancellationToken);
    }
}
