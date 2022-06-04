using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Extensions;

namespace ArmaForces.Arma.Server.Features.Parameters
{
    public class ProcessParameters : IProcessParameters
    {
        public string ProcessPath { get; }

        public bool Client { get; }

        public string? ConnectIpAddress { get; }

        public string? ConnectPassword { get; }

        public int Port { get; }

        public string ServerCfgPath { get; }

        public string BasicCfgPath { get; }

        public string ProfilePath { get; }

        public string Name { get; }

        public string ModsetName { get; }

        public DateTimeOffset? StartTime { get; private set; }

        public bool FilePatching { get; }

        public bool NetLog { get; }

        public int LimitFPS { get; }

        public bool LoadMissionToMemory { get; }

        public IReadOnlyList<string> ServerMod { get; }

        public IReadOnlyList<string> Mod { get; }

        public ProcessParameters(
            string processPath,
            bool client,
            int port,
            string serverCfgPath,
            string basicCfgPath,
            string profilePath,
            string name,
            string modsetName,
            DateTimeOffset? startTime = null,
            bool filePatching = ParametersDefaults.FilePatching,
            bool netLog = ParametersDefaults.Netlog,
            int fpsLimit = ParametersDefaults.LimitFPS,
            bool loadMissionToMemory = ParametersDefaults.LoadMissionToMemory,
            string? connectIpAddress = null,
            string? connectPassword = null,
            IEnumerable<string?>? serverMods = null,
            IEnumerable<string?>? mods = null)
        {
            ProcessPath = processPath;
            Client = client;
            Port = port;
            ServerCfgPath = serverCfgPath;
            BasicCfgPath = basicCfgPath;
            ProfilePath = profilePath;
            Name = name;
            ModsetName = modsetName;
            StartTime = startTime;
            FilePatching = filePatching;
            NetLog = netLog;
            LimitFPS = fpsLimit;
            LoadMissionToMemory = loadMissionToMemory;
            ConnectIpAddress = connectIpAddress;
            ConnectPassword = connectPassword;
            // TODO: Verify if removing null mod directories is correct
            ServerMod = serverMods?.WhereNotNull().ToList() ?? new List<string>();
            Mod = mods?.WhereNotNull().ToList() ?? new List<string>();
        }

        public ProcessStartInfo GetProcessStartInfo() => new ProcessStartInfo(ProcessPath, GetArguments());

        private string GetArguments()
        {
            return string.Join(
                ' ',
                $"-port={Port}",
                $"\"-config={ServerCfgPath}\"",
                $"\"-cfg={BasicCfgPath}\"",
                $"-profiles=\"{ProfilePath}\"",
                GetSpecialParamsString(),
                $"-modsetName={ModsetName}",
                $"-startTime={StartTime ??= DateTimeOffset.Now:O}",
                GetModsStartupParamString());
        }

        private string GetModsStartupParamString()
            => string.Join(
                ' ',
                GetServerModsStartupParamString(),
                GetRequiredModsStartupParamString());

        private string? GetServerModsStartupParamString()
            => ServerMod.Any()
                ? "\"-serverMod=" + string.Join(";", ServerMod) + "\""
                : null;

        private string? GetRequiredModsStartupParamString()
            => Mod.Any()
                ? "\"-mod=" + string.Join(";", Mod) + "\""
                : null;

        private string GetSpecialParamsString()
        {
            return string.Join(
                ' ',
                GetClientString(),
                GetNameString(),
                GetFilePatchingString(),
                GetNetLogString(),
                GetFpsLimitString(),
                GetLoadMissionToMemoryString());
        }

        private string GetClientString()
            => Client
                ? $"-client -connect={ConnectIpAddress} {GetClientPasswordString()}"
                : string.Empty;

        private string GetClientPasswordString()
            => ConnectPassword is null
                ? string.Empty
                : $"-password={ConnectPassword}";

        private string GetNameString() => $"-name={Name}";

        private string GetFilePatchingString() => FilePatching ? "-filePatching" : string.Empty;

        private string GetNetLogString() => NetLog ? "-netlog" : string.Empty;

        private string GetFpsLimitString() => LimitFPS != 90 ? "-limitFPS=100" : string.Empty;

        private string GetLoadMissionToMemoryString() => LoadMissionToMemory ? "-loadMissionToMemory" : string.Empty;

        public bool Equals(IProcessParameters? other) => Port == other?.Port && ModsetName == other.ModsetName;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcessParameters) obj);
        }

        public override int GetHashCode() => HashCode.Combine(Port, ModsetName);
    }
}
