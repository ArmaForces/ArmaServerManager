using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public interface IArmaProcessManager
    {
        Task<Result<IArmaProcess>> CheckServerIsRestarting(IArmaProcess armaProcess);
    }
}
