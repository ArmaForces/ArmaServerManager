using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Keys.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Keys.IO
{
    internal interface IKeysCopier
    {
        Result DeleteKeys(IReadOnlyCollection<BikeyFile> bikeyFiles);
        Result CopyKeys(string targetDirectory, IReadOnlyCollection<BikeyFile> bikeyFiles);
    }
}
