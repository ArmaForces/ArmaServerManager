﻿using CSharpFunctionalExtensions;

namespace Arma.Server.Features.Server
{
    public interface IServerProcess
    {
        bool IsStopped { get; }
        
        bool IsStartingOrStarted { get; }

        Result Start();

        Result Shutdown();
    }
}
