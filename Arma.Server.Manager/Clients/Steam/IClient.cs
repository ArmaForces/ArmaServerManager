using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arma.Server.Manager.Clients.Steam {
    public interface IClient {
        Task Connect();
        void Disconnect();

        Task Download(int itemId);
        Task Download(IEnumerable<int> itemsIds);
    }
}