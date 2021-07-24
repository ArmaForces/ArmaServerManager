using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public class ArmaProcessDiscoverer : IArmaProcessDiscoverer
    {
        private const string ArmaProcessName = "arma3server";

        private readonly IParametersExtractor _parametersExtractor;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<ArmaProcessDiscoverer> _logger;

        public ArmaProcessDiscoverer(
            IParametersExtractor parametersExtractor,
            IArmaProcessFactory armaProcessFactory,
            ILogger<ArmaProcessDiscoverer> logger)
        {
            _parametersExtractor = parametersExtractor;
            _armaProcessFactory = armaProcessFactory;
            _logger = logger;
        }

        public async Task<Dictionary<int, List<IArmaProcess>>> DiscoverArmaProcesses()
        {
            var foundProcesses = Process.GetProcesses()
                .Where(x => x.ProcessName.Contains(ArmaProcessName))
                .ToList();

            var armaProcesses = new Dictionary<int, List<IArmaProcess>>();

            if (foundProcesses.IsEmpty()) return armaProcesses;
            
            _logger.LogInformation($"Found {{count}} running {ArmaProcessName} processes.", foundProcesses.Count);
            foreach (var armaServerProcess in foundProcesses)
            {
                void AddProcessToDictionary(IArmaProcess process)
                {
                    if (armaProcesses.ContainsKey(process.Parameters.Port))
                    {
                        armaProcesses[process.Parameters.Port].Add(process);
                    }
                    else
                    {
                        armaProcesses.Add(
                            process.Parameters.Port,
                            new List<IArmaProcess> {process});
                    }
                }

                await _parametersExtractor.ExtractParameters(armaServerProcess)
                    .Bind(parameters => CreateArmaProcess(armaServerProcess, parameters))
                    .Tap(AddProcessToDictionary);
            }

            return armaProcesses;
        }

        private Result<IArmaProcess> CreateArmaProcess(
            Process armaServerProcess,
            ProcessParameters parameters)
        {
            return Result.Success(_armaProcessFactory.CreateServerProcess(armaServerProcess, parameters));
        }
    }
}
