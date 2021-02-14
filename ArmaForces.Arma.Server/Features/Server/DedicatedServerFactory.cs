﻿using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Modsets;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class DedicatedServerFactory : IDedicatedServerFactory
    {
        private readonly IServerBuilderFactory _serverBuilderFactory;

        public DedicatedServerFactory(IServerBuilderFactory serverBuilderFactory)
        {
            _serverBuilderFactory = serverBuilderFactory;
        }

        public IDedicatedServer CreateDedicatedServer(
            int port,
            IModset modset,
            int numberOfHeadlessClients)
        {
            var builder = _serverBuilderFactory.CreateServerBuilder();
            return builder
                .OnPort(port)
                .WithModset(modset)
                .WithHeadlessClients(numberOfHeadlessClients)
                .Build();
        }

        public IDedicatedServer CreateDedicatedServer(
            int port,
            IModset modset,
            IServerProcess serverProcess,
            IEnumerable<IServerProcess> headlessProcesses)
        {
            var builder = _serverBuilderFactory.CreateServerBuilder();
            return builder
                .OnPort(port)
                .WithModset(modset)
                .WithServerProcess(serverProcess)
                .WithHeadlessClients(headlessProcesses)
                .Build();
        }
    }
}