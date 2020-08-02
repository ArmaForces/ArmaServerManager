using System;

namespace Arma.Server.Config {
    public class ServerNotFoundException: Exception {
        public ServerNotFoundException()
        {
        }

        public ServerNotFoundException(string message)
            : base(message)
        {
        }

        public ServerNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}