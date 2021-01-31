using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Providers.Keys
{
    public interface IKeysProvider
    {
        Result PrepareKeysForModset(IModset modset);
    }
}
