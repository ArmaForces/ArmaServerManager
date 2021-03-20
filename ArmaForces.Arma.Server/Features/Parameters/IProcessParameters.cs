using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ArmaForces.Arma.Server.Features.Parameters
{
    public interface IProcessParameters : IEquatable<IProcessParameters>
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

        public IReadOnlyList<string> ServerMod { get; }

        public IReadOnlyList<string> Mod { get; }

        ProcessStartInfo GetProcessStartInfo();
    }
}
