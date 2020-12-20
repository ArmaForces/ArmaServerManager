using System.Collections.Generic;
using ArmaForces.Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Providers
{
    public interface IModsetProvider
    {
        Result<IModset> GetModsetByName(string modsetName);

        Result<IEnumerable<IModset>> GetModsets();
    }
}
