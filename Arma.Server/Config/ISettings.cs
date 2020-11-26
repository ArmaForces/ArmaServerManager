using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Arma.Server.Config {
    public interface ISettings {

        /// <summary>
        /// Base address for missions API
        /// </summary>
        string ApiMissionsBaseUrl { get; }

        /// <summary>
        /// Base address for modsets API
        /// </summary>
        string ApiModsetsBaseUrl { get; }

        /// <summary>
        /// Path to Manager directory.
        /// </summary>
        string ManagerDirectory { get; }

        /// <summary>
        /// Name of modset configuration files directory.
        /// </summary>
        string ModsetConfigDirectoryName { get; }

        /// <summary>
        /// Path to mods directory.
        /// </summary>
        string ModsDirectory { get; }

        /// <summary>
        /// Name for mods cache file.
        /// </summary>
        string ModsManagerCacheFileName { get; }
        
        /// <summary>
        /// Path to server configuration files directory.
        /// </summary>
        string ServerConfigDirectory { get; }

        /// <summary>
        /// Path pointing to server root folder.
        /// </summary>
        string ServerDirectory { get; }


        /// <summary>
        /// Path to server executable file.
        /// </summary>
        string ServerExecutable { get; }

        /// <summary>
        /// Server executable file name, eg. "arma3server_x64.exe".
        /// </summary>
        string ServerExecutableName { get; }

        /// <summary>
        /// User name for Steam downloading.
        /// </summary>
        string SteamUser { get; }

        /// <summary>
        /// Password for Steam downloading.
        /// </summary>
        string SteamPassword { get; }

        /// <summary>
        /// Loads settings from configuration.
        /// </summary>
        /// <returns>Result</returns>
        Result LoadSettings();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Result ReloadSettings();

        Task<Result> ReloadSettings(ISettings settings);
    }
}