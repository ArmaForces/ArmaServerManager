using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Keys.Models;

namespace ArmaForces.Arma.Server.Features.Keys.Finder
{
    internal interface IKeysFinder
    {
        List<BikeyFile> GetKeysFromDirectory(string? directory);
    }
}
