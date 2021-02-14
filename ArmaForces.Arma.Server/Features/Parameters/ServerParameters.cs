using System.Diagnostics;
using ArmaForces.Arma.Server.Constants;

namespace ArmaForces.Arma.Server.Features.Parameters
{
    public class ServerParameters
    {
        public string ProcessPath { get; }

        public bool Client { get; }

        public int Port { get; }

        public string ServerCfgPath { get; }

        public string BasicCfgPath { get; }

        public string ProfilePath { get; }

        public string Name { get; }

        public string ModsetName { get; }

        public bool FilePatching { get; }

        public bool NetLog { get; }

        public int LimitFPS { get; }

        public bool LoadMissionToMemory { get; }

        public ServerParameters(
            string processPath,
            bool client,
            int port,
            string serverCfgPath,
            string basicCfgPath,
            string profilePath,
            string name,
            string modsetName,
            bool filePatching = ParametersDefaults.FilePatching,
            bool netLog = ParametersDefaults.Netlog,
            int fpsLimit = ParametersDefaults.LimitFPS,
            bool loadMissionToMemory = ParametersDefaults.LoadMissionToMemory)
        {
            ProcessPath = processPath;
            Client = client;
            Port = port;
            ServerCfgPath = serverCfgPath;
            BasicCfgPath = basicCfgPath;
            ProfilePath = profilePath;
            Name = name;
            ModsetName = modsetName;
            FilePatching = filePatching;
            NetLog = netLog;
            LimitFPS = fpsLimit;
            LoadMissionToMemory = loadMissionToMemory;
        }

        public ProcessStartInfo GetProcessStartInfo() => new ProcessStartInfo(ProcessPath, GetArguments());

        private string GetArguments()
        {
            // TODO: Get arguments list here
            return string.Empty;
        }
    }
}
