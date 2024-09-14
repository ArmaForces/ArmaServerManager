
namespace ArmaForces.ArmaServerManager.Api.Servers.DTOs
{
    /// <summary>
    /// Request to start/stop headless clients.
    /// If number of running HCs is lower than specified in the request, new HCs will be started. Otherwise some HCs will be shut down.
    /// Has no effect if number of running HCs matches desired HCs count.
    /// </summary>
    public class HeadlessSetRequestDto
    {
        /// <summary>
        /// Number of headless clients that should be running.
        /// </summary>
        public required int Count { get; set; }
    }
}