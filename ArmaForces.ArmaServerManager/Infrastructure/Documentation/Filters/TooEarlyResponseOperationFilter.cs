using ArmaForces.ArmaServerManager.Common;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ArmaForces.ArmaServerManager.Infrastructure.Documentation.Filters
{
    /// <summary>
    /// Changes name of 425 HTTP status code from "Client Error" to "Too Early".
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TooEarlyResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var tooEarlyResponseDefined = operation.Responses.TryGetValue(
                StatusCodesExtended.Status425TooEarly.ToString(),
                out var openApiResponse);

            if (tooEarlyResponseDefined && openApiResponse != null)
            {
                openApiResponse.Description = "Too Early";
            }
        }
    }
}