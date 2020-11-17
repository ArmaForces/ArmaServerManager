using Arma.Server.Features.Server;

namespace Arma.Server.Manager.Features.Server.DTOs
{
    public class ServerStatus
    {
        private readonly IDedicatedServer _dedicatedServer;

        public ServerStatus(IDedicatedServer dedicatedServer, int port)
        {
            _dedicatedServer = dedicatedServer;
            Port = port;
        }

        public int? HeadlessClientsConnected => _dedicatedServer?.HeadlessClientsConnected;

        public bool IsServerRunning => _dedicatedServer?.IsServerStarted ?? false;

        public string ModsetName => _dedicatedServer?.Modset.Name;

        public int Port { get; }
    }
}
