using System.Collections.Generic;
using Arma.Server.Mod;

namespace Arma.Server.Modlist {
    public interface IModlist {
        string GetId();
        string GetName();
        List<Mod.Mod> GetModsList();
    }
}