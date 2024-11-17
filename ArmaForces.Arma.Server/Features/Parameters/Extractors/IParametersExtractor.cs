using System.Diagnostics;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Parameters.Extractors
{
    public interface IParametersExtractor
    {
       Task<Result<ProcessParameters, IError>> ExtractParameters(Process process);

       Task<Result<ProcessParameters, IError>> ExtractParameters(string commandLine);
    }
}
