using System;

namespace ArmaForces.Arma.Server.Exceptions
{
    public class ServerMismatchException : Exception
    {
        public ServerMismatchException()
            : base()
        {
        }

        public ServerMismatchException(string message)
            : base(message)
        {
        }

        public ServerMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
