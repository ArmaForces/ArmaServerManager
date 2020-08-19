using System.Collections.Generic;

namespace Arma.Server.Modlist {
    public interface IBla {
        List<Modlist> GetModlists();

        Modlist GetModlistData(Modlist modlist);

        Modlist GetModlistData(string modlistId);
    }
}