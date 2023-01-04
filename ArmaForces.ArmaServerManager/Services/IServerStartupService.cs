using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    public interface IServerStartupService
    {
        /// <summary>
        /// TODO: create documentation
        /// </summary>
        /// <param name="modsetName"></param>
        /// <param name="headlessClients"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> StartServer(string modsetName, int headlessClients, CancellationToken cancellationToken);

        /// <summary>
        /// TODO: create documentation
        /// </summary>
        /// <param name="modset"></param>
        /// <param name="headlessClients"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> StartServer(Modset modset, int headlessClients, CancellationToken cancellationToken);

        /// <summary>
        /// TODO: create documentation
        /// </summary>
        /// <param name="port"></param>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> ShutdownServer(
            int port,
            bool force = false,
            CancellationToken? cancellationToken = null);
    }
}
