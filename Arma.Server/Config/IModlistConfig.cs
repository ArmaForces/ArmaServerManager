namespace Arma.Server.Config {
    public interface IModlistConfig : IConfig {

        /// <summary>
        /// Path to config.json file.
        /// </summary>
        new string ConfigJson { get; }

        /// <summary>
        /// Path to Headless Client profile directory.
        /// </summary>
        string HCProfileDirectory { get; }

        /// <summary>
        /// Modlist name for given config.
        /// </summary>
        string ModlistName { get; }

        /// <summary>
        /// Path to Server profile directory.
        /// </summary>
        string ServerProfileDirectory { get; }
    }
}