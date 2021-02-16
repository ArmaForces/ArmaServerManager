using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Server.DTOs;
using ArmaForces.Arma.Server.Providers.Keys;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("ArmaForces.Arma.Server.Tests")]

namespace ArmaForces.Arma.Server.Features.Server
{
    public class DedicatedServer : IDedicatedServer
    {
        private readonly IModsetConfig _modsetConfig;
        private readonly ILogger<DedicatedServer> _logger;
        private readonly IKeysProvider _keysProvider;

        private readonly IArmaProcess _armaProcess;
        private readonly List<IArmaProcess> _headlessProcesses;
        
        public DedicatedServer(
            int port,
            IModset modset,
            IModsetConfig modsetConfig,
            IKeysProvider keysProvider,
            IArmaProcess armaProcess,
            IEnumerable<IArmaProcess> headlessClients,
            ILogger<DedicatedServer> logger)
        {
            Port = port;
            _keysProvider = keysProvider;
            Modset = modset;
            _modsetConfig = modsetConfig;
            _armaProcess = armaProcess;
            _headlessProcesses = headlessClients.ToList();
            _logger = logger;
        }

        public int Port { get; }

        public IModset Modset { get; }

        public int HeadlessClientsConnected => _headlessProcesses.Count;

        public bool IsServerStopped => _armaProcess?.IsStopped ?? true;

        public event EventHandler Disposed;

        public void Dispose()
        {
            Shutdown();

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public Result Start()
        {
            _logger.LogTrace("Starting server on port {port} with {modsetName} modset.", Port, Modset.Name);

            return _modsetConfig.CopyConfigFiles()
                .Bind(() => _keysProvider.PrepareKeysForModset(Modset))
                .Bind(() => _armaProcess.Start())
                .Bind(() => _headlessProcesses.Select(x => x.Start())
                    .Combine());
        }

        public Result Shutdown()
        {
            _armaProcess?.Shutdown();
            foreach (var headlessProcess in _headlessProcesses)
            {
                headlessProcess.Shutdown();
            }

            _logger.LogTrace("Server shutdown completed");

            return Result.Success();
        }

        

        public async Task<ServerStatus> GetServerStatusAsync(CancellationToken cancellationToken) 
            => await ServerStatus.GetServerStatus(this, cancellationToken);
    }
}
