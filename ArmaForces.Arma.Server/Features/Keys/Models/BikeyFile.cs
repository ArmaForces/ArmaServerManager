namespace ArmaForces.Arma.Server.Features.Keys.Models
{
    /// <summary>
    /// Encapsulates basic bikey file information.
    /// </summary>
    public readonly struct BikeyFile
    {
        public BikeyFile(string path)
        {
            Path = path;
            Target = "Unknown";
        }
        
        public BikeyFile(string path, string target)
        {
            Path = path;
            Target = target;
        }

        /// <summary>
        /// Path to bikey file.
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        /// Indicates what the bikey is for.
        /// </summary>
        public string Target { get; }
    }
}
