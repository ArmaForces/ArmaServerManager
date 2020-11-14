using System.Collections.Generic;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Providers
{
    public interface IModsetProvider
    {
        Result<IModset> GetModsetByName(string modsetName);

        Result<IEnumerable<IModset>> GetModsets();
    }
}
