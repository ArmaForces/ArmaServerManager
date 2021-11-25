namespace ArmaForces.Arma.Server.Features.Mods
{
    public enum ModStatus
    {
        /// <summary>
        /// Mod is ok, can be run.
        /// </summary>
        Active,
        
        /// <summary>
        /// Mod is old, not supported or there is a better alternative available.
        /// <inheritdoc cref="Broken"/>
        /// </summary>
        Deprecated,
        
        /// <summary>
        /// Mod can be run but may not work correctly.
        /// </summary>
        Broken,
        
        /// <summary>
        /// Mod should not be run.
        /// </summary>
        Disabled
    }
}
