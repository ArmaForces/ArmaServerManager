namespace Arma.Server.Config {
    public interface IModsetConfig : IConfig {

        /// <summary>
        /// Path to config.json file.
        /// </summary>
        new string ConfigJson { get; }

        /// <summary>
        /// Path to Headless Client profile directory.
        /// </summary>
        string HCProfileDirectory { get; }

        /// <summary>
        /// Modset name for given config.
        /// </summary>
        string ModsetName { get; }

        /// <summary>
        /// Path to Server profile directory.
        /// </summary>
        string ServerProfileDirectory { get; }
    }
}