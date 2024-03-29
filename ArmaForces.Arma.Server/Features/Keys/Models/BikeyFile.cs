﻿namespace ArmaForces.Arma.Server.Features.Keys.Models
{
    /// <summary>
    /// Encapsulates basic bikey file information.
    /// </summary>
    public readonly struct BikeyFile
    {
        /// <param name="path">Path to bikey file.</param>
        public BikeyFile(string path)
        {
            Path = path;
            Target = "Unknown";
        }
        
        /// <param name="path">Path to bikey file.</param>
        /// <param name="target">What the bikey is for (e.g., addon/mod name).</param>
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
        /// Bikey file name.
        /// </summary>
        public string FileName => System.IO.Path.GetFileName(Path);
        
        /// <summary>
        /// Indicates what the bikey is for.
        /// </summary>
        public string Target { get; }
    }
}
