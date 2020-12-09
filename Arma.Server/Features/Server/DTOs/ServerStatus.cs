using System.Net;
using System.Net.Sockets;

namespace Arma.Server.Features.Server.DTOs
{
    public class ServerStatus
    {
        private readonly IDedicatedServer _dedicatedServer;
        private readonly A2SInfo _serverInfo;

        public ServerStatus()
        {
        }

        public ServerStatus(IDedicatedServer dedicatedServer)
        {
            _dedicatedServer = dedicatedServer;
            _serverInfo = GetServerInfo(dedicatedServer.Port);
        }

        public int? HeadlessClientsConnected => _dedicatedServer?.HeadlessClientsConnected;

        public bool IsServerStarting => !IsServerRunning && (!_dedicatedServer?.IsServerStopped ?? false);

        public bool IsServerRunning => !(_serverInfo is null);

        public string ModsetName => _dedicatedServer?.Modset.Name;

        public string Name => _serverInfo?.Name;

        public string Map => _serverInfo?.Map;

        public int Players => _serverInfo?.Players ?? 0;

        public int PlayersMax => _serverInfo?.MaxPlayers ?? 0;

        public int Port => _dedicatedServer?.Port ?? 0;

        private static A2SInfo GetServerInfo(int port)
        {
            try
            {
                return new A2SInfo(new IPEndPoint(IPAddress.Loopback, port + 1));
            }
            catch (SocketException)
            {
                return null;
            }
        }
    }
}
