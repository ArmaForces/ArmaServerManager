using System.Collections.Generic;
using Arma.Server.Manager.Clients.Modsets.Entities;

namespace Arma.Server.Manager.Clients.Modsets {
    public interface IApiModsetClient {
        List<WebModset> GetModsets();

        WebModset GetModsetDataByName(string name);

        WebModset GetModsetDataByModset(WebModset webModset);

        WebModset GetModsetDataById(string id);
    }
}