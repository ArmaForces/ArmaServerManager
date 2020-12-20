using Microsoft.Win32;

namespace ArmaForces.Arma.Server.Config {
    public class RegistryReader : IRegistryReader {
        public object GetValueFromLocalMachine(string subKey, string value)
            => Registry.LocalMachine
                .OpenSubKey(subKey)
                ?.GetValue(value);
    }
}