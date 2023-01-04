using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Modsets
{
    public interface IModsetProvider
    {
        Task<Result<Modset>> GetModsetByName(string modsetName);

        Task<Result<List<Modset>>> GetModsets();
    }
}
