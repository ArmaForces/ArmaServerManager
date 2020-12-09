using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Arma.Server.Features.Server.DTOs
{
    public class ServerStatus
    {
        private readonly IDedicatedServer _dedicatedServer;
        private readonly A2SInfo _serverInfo;

        public ServerStatus()
        {
        }

        public ServerStatus(IDedicatedServer dedicatedServer, A2SInfo serverInfo)
        {
            _dedicatedServer = dedicatedServer;
            _serverInfo = serverInfo;
        }

        public static async Task<ServerStatus> GetServerStatus(
            IDedicatedServer dedicatedServer,
            CancellationToken cancellationToken)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Loopback, dedicatedServer.Port + 1);
            var serverInfo = await A2SInfo.GetServerInfoAsync(ipEndPoint, cancellationToken);

            return new ServerStatus(dedicatedServer, serverInfo);
        }

        public int? HeadlessClientsConnected => _dedicatedServer?.HeadlessClientsConnected;

        public bool IsServerStarting => !IsServerRunning && (!_dedicatedServer?.IsServerStopped ?? false);

        public bool IsServerRunning => !(_serverInfo is null);

        public bool IsServerStopped => _dedicatedServer?.IsServerStopped ?? true;

        public string ModsetName => _dedicatedServer?.Modset.Name;

        public string Name => _serverInfo?.Name;

        public string Map => _serverInfo?.Map;

        public int Players => _serverInfo?.Players ?? 0;

        public int PlayersMax => _serverInfo?.MaxPlayers ?? 0;

        public int Port => _dedicatedServer?.Port ?? 0;
    }
}
