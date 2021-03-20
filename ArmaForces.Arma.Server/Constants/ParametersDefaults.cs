// ReSharper disable IdentifierTypo
namespace ArmaForces.Arma.Server.Constants
{
    /// <summary>
    /// Default value for server command line parameters when running with this library.
    /// These might not be the same as default used by Arma.
    /// </summary>
    public class ParametersDefaults
    {
        public const string ConnectIpAddress = "127.0.0.1";

        public const int Port = 2302;
        
        public const bool FilePatching = false;

        public const bool Netlog = false;
        
        public const int LimitFPS = 90;

        public const bool LoadMissionToMemory = true;
    }
}
