using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                GetLogger(context).LogInformation("No API Key provided");
                context.Result = CreateFailedResult(HttpStatusCode.Unauthorized, "Api Key was not provided.");
                return;
            }
            
            if (!GetApiKeys(context)
                .Any(x => x.Equals(extractedApiKey)))
            {
                GetLogger(context).LogWarning("Invalid API Key used: {Key}", extractedApiKey);
                context.Result = CreateFailedResult(HttpStatusCode.Unauthorized, "Api Key is not valid.");
                return;
            }

            var apiKeySha256 = GetApiKeySha256(extractedApiKey);

            var actionDetails = GetActionDetails(context.ActionDescriptor, context.ActionArguments);
            
            GetLogger(context).LogInformation("Valid API Key {Key} used for route {Route}, executing action {Action}", apiKeySha256, context.ActionDescriptor.AttributeRouteInfo?.Template, actionDetails);

            await next();
        }

        private static string GetActionDetails(ActionDescriptor actionDescriptor, IDictionary<string, object?> actionArguments)
        {
            var parameters = string.Join(", ", actionArguments.Select(x => $"{x.Key}={x.Value}"));
            return $"{actionDescriptor.AttributeRouteInfo?.Name}({parameters})";
        }

        private static IEnumerable<string> GetApiKeys(ActionContext context)
            => context.HttpContext.RequestServices
                .GetRequiredService<IApiKeyProvider>()
                .GetAcceptedApiKeys();

        private static ILogger<ApiKeyAttribute> GetLogger(ActionContext context)
            => context.HttpContext.RequestServices
                .GetRequiredService<ILogger<ApiKeyAttribute>>();

        private static ContentResult CreateFailedResult(HttpStatusCode statusCode, string errorMessage)
            => new ContentResult
            {
                ContentType = MediaTypeNames.Application.Json,
                Content = CreateErrorContent(errorMessage),
                StatusCode = (int) statusCode
            };

        private static string GetApiKeySha256(string extractedApiKey)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(extractedApiKey)));

        private static string CreateErrorContent(string errorMessage)
            => JsonSerializer.Serialize(new ErrorContent {ErrorMessage = errorMessage});

        private class ErrorContent
        {
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}
