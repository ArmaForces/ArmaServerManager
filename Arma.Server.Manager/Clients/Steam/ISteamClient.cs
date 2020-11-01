using System.Threading;
using System.Threading.Tasks;
using BytexDigital.Steam.ContentDelivery;

namespace Arma.Server.Manager.Clients.Steam {
    /// <summary>
    /// Handles connection to Steam Servers and downloading of mods.
    /// </summary>
    public interface ISteamClient {
        SteamContentClient ContentClient { get; }

        /// <summary>
        /// Connects to Steam Servers.
        /// </summary>
        /// /// <param name="cancellationToken"><see cref="CancellationToken"/> used for safe connection aborting.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task EnsureConnected(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects from Steam Servers.
        /// </summary>
        void Disconnect();
    }
}