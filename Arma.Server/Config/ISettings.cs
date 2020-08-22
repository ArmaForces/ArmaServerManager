namespace Arma.Server.Config {
    public interface ISettings {

        /// <summary>
        /// Name of modlist configuration files directory.
        /// </summary>
        string ModlistConfigDirectoryName { get; }

        /// <summary>
        /// Path to mods directory.
        /// </summary>
        string ModsDirectory { get; }
        
        /// <summary>
        /// Name of server configuration files directory.
        /// </summary>
        string ServerConfigDirectoryName { get; }

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
    }
}