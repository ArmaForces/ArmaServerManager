using System.Diagnostics;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Parameters
{
    public interface IParametersExtractor
    {
       Task<Result<ProcessParameters>> ExtractParameters(Process process);

       Task<Result<ProcessParameters>> ExtractParameters(string commandLine);
    }
}
