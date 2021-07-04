using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Processes
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
            var armaProcesses = await _armaProcessDiscoverer.DiscoverArmaProcesses();
            var newArmaProcess = armaProcesses
                .SingleOrDefault(x => x.Value.Exists(process => Equals(process.Parameters, armaProcess.Parameters)))
                .Value?
                .SingleOrDefault(process => process.ProcessType == ArmaProcessType.Server);

            return newArmaProcess is null
                ? Result.Failure<IArmaProcess>("Server process did not restart.")
                : Result.Success(newArmaProcess);
        }
    }
}
