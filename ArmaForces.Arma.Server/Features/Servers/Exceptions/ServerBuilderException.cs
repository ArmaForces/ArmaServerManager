using System;

namespace ArmaForces.Arma.Server.Features.Servers.Exceptions
{
    public class ServerBuilderException : Exception
    {
        public ServerBuilderException(string validationResultError) : base(validationResultError)
        {
        }
    }
}
