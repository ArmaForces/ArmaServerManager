﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public interface IArmaProcessDiscoverer
    {
        Task<Dictionary<int, List<IArmaProcess>>> DiscoverArmaProcesses();
    }
}
