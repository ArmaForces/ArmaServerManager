using System.Linq;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ArmaForces.ArmaServerManager.Infrastructure.Documentation.Filters
{
    /// <summary>
    /// Adds default 401 response for all operations with <see cref="ApiKeyAttribute"/>.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ApiKeyUnauthorizedResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<ApiKeyAttribute>();

            if (authAttributes?.IsEmpty() ?? true) return;
            
            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse {Description = "Unauthorized"});
        }
    }
}