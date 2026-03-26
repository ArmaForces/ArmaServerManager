using System.Net.Mime;
using ArmaForces.Arma.Server.Common.Errors;
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
        /// Creates failed result according to given <paramref name="error"/>.
        /// </summary>
        /// <param name="error">Error that occurred.</param>
        protected ObjectResult Failure(IError error)
            => Problem(
                detail: error.Message,
                statusCode: error.Code.Value);
        
        /// <summary>
        /// Creates 202 Accepted with given <paramref name="jobId"/>.
        /// </summary>
        /// <param name="jobId">Id of the accepted job.</param>
        protected ObjectResult JobAccepted(int jobId)
            => Accepted(jobId);

        /// <summary>
        /// Creates 400 Bad Request with given <paramref name="errorMessage"/>.
        /// </summary>
        /// <param name="error">Error that occurred</param>
        protected ObjectResult ModsetNotFound(IError error)
            => BadRequest(error.Message);
        
        /// <summary>
        /// Creates 425 Too Early.
        /// </summary>
        /// <param name="error">Error details.</param>
        protected ObjectResult TooEarly(IError error)
            => StatusCode(StatusCodesExtended.Status425TooEarly,
                $"Similar request is already in processing. Please wait. Error: {error}");
    }
}