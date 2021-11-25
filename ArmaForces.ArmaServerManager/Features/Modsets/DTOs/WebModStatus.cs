using System.Runtime.Serialization;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs
{
    public enum WebModStatus
    {
        /// <summary>
        /// Mod is old, not supported or there is a better alternative available.
        /// </summary>
        [EnumMember(Value = "deprecated")]
        Deprecated,
        
        /// <summary>
        /// Mod is broken. Use at your own risk!
        /// </summary>
        [EnumMember(Value = "broken")]
        Broken,
        
        /// <summary>
        /// Mod is disabled and excluded from client download.
        /// </summary>
        [EnumMember(Value = "disabled")]
        Disabled
    }
}
