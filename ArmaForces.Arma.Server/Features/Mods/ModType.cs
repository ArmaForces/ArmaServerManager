namespace ArmaForces.Arma.Server.Features.Mods
{
    public enum ModType
    {
        /// <summary>
        /// Loaded only on server.
        /// </summary>
        ServerSide,

        /// <summary>
        /// Loaded on server and on clients.
        /// </summary>
        Required,

        /// <summary>
        /// Loaded on server and can be loaded on clients.
        /// </summary>
        Optional,

        /// <summary>
        /// Not loaded on server and can be loaded on clients.
        /// </summary>
        ClientSide
    }
}