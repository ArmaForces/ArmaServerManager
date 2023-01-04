namespace ArmaForces.ArmaServerManager.Api.Status.DTOs
{
    /// <summary>
    /// Short application status.
    /// Intended for quick insights what the app is doing at the moment.
    /// </summary>
    public enum AppStatus
    {
        /// <summary>
        /// No job in progress.
        /// </summary>
        Idle,
        
        /// <summary>
        /// Mods are updating.
        /// </summary>
        UpdatingMods,
        
        /// <summary>
        /// Server is starting.
        /// </summary>
        StartingServer,
        
        /// <summary>
        /// Something other.
        /// </summary>
        Busy
    }
}