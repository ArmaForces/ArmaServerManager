using System;

namespace ArmaForces.Arma.Server.Exceptions {
    public class ServerNotFoundException : Exception {
        public ServerNotFoundException() {
        }

        public ServerNotFoundException(string message)
            : base(message) {
        }

        public ServerNotFoundException(string message, Exception inner)
            : base(message, inner) {
        }
    }
}