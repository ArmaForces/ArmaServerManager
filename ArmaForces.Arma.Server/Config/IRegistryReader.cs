namespace ArmaForces.Arma.Server.Config
{
    public interface IRegistryReader
    {
        /// <summary>
        /// Reads value from registry.
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        object? GetValueFromLocalMachine(string subKey, string value);
    }
}