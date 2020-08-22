using System.Collections.Generic;

namespace Arma.Server.Modlist {
    public interface IApiModlistDataService {
        List<Modlist> GetModlists();

        Modlist GetModlistDataByName(string name);

        Modlist GetModlistDataByModlist(Modlist modlist);

        Modlist GetModlistDataById(string id);
    }
}