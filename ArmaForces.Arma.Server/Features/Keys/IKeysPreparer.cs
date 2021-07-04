using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Keys
{
    public interface IKeysPreparer
    {
        Result PrepareKeysForModset(IModset modset);
    }
}
