using System;
using Microsoft.Win32;

namespace ArmaForces.Arma.Server.Config
{
    internal class RegistryReader : IRegistryReader
    {
        public object? GetValueFromLocalMachine(string subKey, string value)
            => OperatingSystem.IsWindows()
                ? Registry.LocalMachine
                    .OpenSubKey(subKey)?
                    .GetValue(value)
                : null;
    }
}