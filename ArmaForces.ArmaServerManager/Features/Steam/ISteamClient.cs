using System.Threading;
using System.Threading.Tasks;
using BytexDigital.Steam.ContentDelivery;

namespace ArmaForces.ArmaServerManager.Features.Steam
{
    /// <summary>
    ///     Handles connection to Steam Servers and downloading of mods.
    /// </summary>
    public interface ISteamClient
    {
        /// <summary>
        ///     Content client for downloading.
        /// </summary>
        SteamContentClient ContentClient { get; }

        /// <summary>
        ///     Ensures client is connected to Steam Servers.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> used for safe connection aborting.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        Task EnsureConnected(CancellationToken cancellationToken);

        /// <summary>
        ///     Disconnects from Steam Servers.
        /// </summary>
        void Disconnect();
    }
}
