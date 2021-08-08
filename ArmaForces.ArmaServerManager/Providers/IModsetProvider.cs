using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Providers
{
    public interface IModsetProvider
    {
        Task<Result<IModset>> GetModsetByName(string modsetName);

        Task<Result<List<IModset>>> GetModsets();
    }
}
