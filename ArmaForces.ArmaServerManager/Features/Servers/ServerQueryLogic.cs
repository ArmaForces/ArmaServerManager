using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Features.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Servers.Providers;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Servers
{
    internal class ServerQueryLogic : IServerQueryLogic
    {
        private readonly IServerProvider _serverProvider;
        private readonly IServerStatusFactory _serverStatusFactory;

        public ServerQueryLogic(IServerProvider serverProvider, IServerStatusFactory serverStatusFactory)
        {
            _serverProvider = serverProvider;
            _serverStatusFactory = serverStatusFactory;
        }

        public Result<IDedicatedServer> GetServer(int port)
        {
            var server = _serverProvider.GetServer(port); 
            return server != null
                ? Result.Success(server)
                : Result.Failure<IDedicatedServer>($"Server not found on port {port}");
        }

        public async Task<ServerStatus> GetServerStatus(int port, CancellationToken cancellationToken)
        {
            var server = _serverProvider.GetServer(port);

            if (server is null) return new ServerStatus();

            return await _serverStatusFactory.GetServerStatus(server, cancellationToken);
        }

        public async Task<List<ServerStatus>> GetAllServersStatus(CancellationToken cancellationToken)
        {
            var tasks = _serverProvider.GetServers()
                .Select(x => _serverStatusFactory.GetServerStatus(x, cancellationToken))
                .ToList();

            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}