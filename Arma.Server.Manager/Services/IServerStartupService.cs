using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Services
{
    public interface IServerStartupService
    {
        /// <summary>
        /// TODO: create documentation
        /// </summary>
        /// <param name="modsetName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> StartServer(string modsetName, CancellationToken cancellationToken);

        /// <summary>
        /// TODO: create documentation
        /// </summary>
        /// <param name="modset"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> StartServer(IModset modset, CancellationToken cancellationToken);

        /// <summary>
        /// TODO: create documentation
        /// </summary>
        /// <param name="port"></param>
        /// <param name="force"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> ShutdownServer(
            int port,
            bool force,
            CancellationToken cancellationToken);
    }
}
