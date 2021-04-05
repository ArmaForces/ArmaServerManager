using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Infrastructure.Authentication
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyName = "ApiKey";
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyName, out var extractedApiKey))
            {
                context.Result = CreateFailedResult(HttpStatusCode.Unauthorized, "Api Key was not provided.");
                return;
            }
            
            if (!GetApiKeys(context)
                .Any(x => x.Equals(extractedApiKey)))
            {
                context.Result = CreateFailedResult(HttpStatusCode.Unauthorized, "Api Key is not valid.");
                return;
            }

            await next();
        }

        private static IEnumerable<string> GetApiKeys(ActionContext context)
        {
            return context.HttpContext.RequestServices
                    // This should never be null
                    .GetService<IApiKeyProvider>()!
                    .GetAcceptedApiKeys();
        }

        private static ContentResult CreateFailedResult(HttpStatusCode statusCode, string errorMessage)
            => new ContentResult
            {
                ContentType = "application/json",
                Content = CreateErrorContent(errorMessage),
                StatusCode = (int) statusCode
            };

        private static string CreateErrorContent(string errorMessage)
            => JsonConvert.SerializeObject(new ErrorContent {ErrorMessage = errorMessage});

        private class ErrorContent
        {
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}
