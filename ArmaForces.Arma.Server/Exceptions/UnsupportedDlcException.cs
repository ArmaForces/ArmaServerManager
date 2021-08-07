using System;
using ArmaForces.Arma.Server.Features.Dlcs.Constants;

namespace ArmaForces.Arma.Server.Exceptions
{
    public class UnsupportedDlcException : Exception
    {
        public DlcAppId Dlc { get; }

        public UnsupportedDlcException(DlcAppId dlc) : this(dlc, GetDefaultMessage(dlc)) {}

        public UnsupportedDlcException(DlcAppId dlc, string message) : base(message)
        {
            Dlc = dlc;
        }

        private static string GetDefaultMessage(DlcAppId dlc) => $"DLC {dlc} is not supported.";
    }
}
