using System.Collections.Generic;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Keys.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Keys.IO
{
    internal interface IKeysCopier
    {
        UnitResult<IError> DeleteKeys(IReadOnlyCollection<BikeyFile> bikeyFiles);
        
        UnitResult<IError> CopyKeys(string targetDirectory, IReadOnlyCollection<BikeyFile> bikeyFiles);
    }
}
