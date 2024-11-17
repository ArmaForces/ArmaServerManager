using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Keys
{
    public interface IKeysPreparer
    {
        UnitResult<IError> PrepareKeysForModset(Modset modset);
    }
}
