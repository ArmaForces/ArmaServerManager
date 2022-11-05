using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Servers;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Servers
{
    /// <summary>
    /// Allows servers management with query, start and shutdown.
    /// </summary>
    public interface IServerCommandLogic
    {
        /// <summary>
        /// Shuts down server on given <paramref name="port"/>.
        /// </summary>
        /// <param name="port">Port of a server to shut down.</param>
        /// <param name="force">Force shutdown even if players are on the server.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success if server was shut down.</returns>
        Task<Result> ShutdownServer(int port, bool force = false, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Starts server with given parameters.
        /// </summary>
        /// <param name="port">Port on which server should be started.</param>
        /// <param name="headlessClients">Number of Headless Clients for server.</param>
        /// <param name="modset">Modset which should be loaded on the server.</param>
        /// <returns>Successful result with started server reference if server is started.</returns>
        Result<IDedicatedServer> StartServer(int port, int headlessClients, Modset modset);

        /// <summary>
        /// Adds or removes headless clients for server on given <paramref name="port"/>. 
        /// </summary>
        /// <param name="port">Server port.</param>
        /// <param name="desiredHcCount">Desired number of headless clients.</param>
        /// <returns>Successful result if headless clients were started.</returns>
        Task<Result> SetHeadlessClients(int port, int desiredHcCount);
    }
}