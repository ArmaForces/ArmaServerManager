using System;

namespace ArmaForces.Arma.Server.Exceptions
{
    public class ServerRunningException : Exception
    {
        private const string DefaultMessage = "Cannot start a running server.";

        public ServerRunningException()
            : base(DefaultMessage)
        {
        }

        public ServerRunningException(string message)
            : base (message)
        {
        }

        public ServerRunningException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
