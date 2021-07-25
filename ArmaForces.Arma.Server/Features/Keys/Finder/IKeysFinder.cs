using System.Collections.Generic;

namespace ArmaForces.Arma.Server.Features.Keys.Finder
{
    internal interface IKeysFinder
    {
        List<string> GetKeysFromDirectory(string? directory);
    }
}
