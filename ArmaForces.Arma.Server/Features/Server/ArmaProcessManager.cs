﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class ArmaProcessManager : IArmaProcessManager
    {
        private readonly IArmaProcessDiscoverer _armaProcessDiscoverer;

        public ArmaProcessManager(IArmaProcessDiscoverer armaProcessDiscoverer)
        {
            _armaProcessDiscoverer = armaProcessDiscoverer;
        }

        public async Task<Result<IArmaProcess>> CheckServerIsRestarting(IArmaProcess armaProcess)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            var armaProcesses = await _armaProcessDiscoverer.DiscoverArmaProcesses();
            var newArmaProcess = armaProcesses
                .SingleOrDefault(x => x.Value.Exists(process => process.Parameters == armaProcess.Parameters))
                .Value
                .SingleOrDefault(process => process.ProcessType == ArmaProcessType.Server);

            return newArmaProcess is null
                ? Result.Failure<IArmaProcess>("Server process did not restart.")
                : Result.Success(newArmaProcess);
        }
    }

    public interface IArmaProcessManager
    {
        Task<Result<IArmaProcess>> CheckServerIsRestarting(IArmaProcess armaProcess);
    }
}
