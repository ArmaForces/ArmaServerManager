using Microsoft.AspNetCore.Http;

namespace ArmaForces.ArmaServerManager.Common
{
    /// <summary>
    /// Additional status codes not included in original <see cref="StatusCodes"/>.
    /// </summary>
    public static class StatusCodesExtended
    {
        /// <summary>
        /// The server is unwilling to risk processing a request that might be replayed.
        /// </summary>
        public const int Status425TooEarly = 425;
    }
}