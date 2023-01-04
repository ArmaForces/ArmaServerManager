using System.Net.Mime;
using ArmaForces.ArmaServerManager.Common;
using Microsoft.AspNetCore.Mvc;

namespace ArmaForces.ArmaServerManager.Api
{
    /// <summary>
    /// Base controller for all Manager controllers.
    /// Provides handy methods for quick responses.
    /// </summary>
    [Produces(MediaTypeNames.Application.Json)]
    public abstract class ManagerControllerBase : ControllerBase
    {
        /// <summary>
        /// Creates 202 Accepted with given <paramref name="jobId"/>.
        /// </summary>
        /// <param name="jobId">Id of the accepted job.</param>
        protected ObjectResult JobAccepted(int jobId)
            => Accepted(jobId);
        
        /// <summary>
        /// Creates 425 Too Early.
        /// </summary>
        /// <param name="error">Error details.</param>
        protected ObjectResult TooEarly(string error)
            => StatusCode(StatusCodesExtended.Status425TooEarly,
                $"Similar request is already in processing. Please wait. Error: {error}");
    }
}