using System.Collections.Generic;

namespace Arma.Server.Manager.Modset {
    public interface IApiModsetClient {
        List<Modset> GetModsets();

        Modset GetModsetDataByName(string name);

        Modset GetModsetDataByModset(Modset modset);

        Modset GetModsetDataById(string id);
    }
}