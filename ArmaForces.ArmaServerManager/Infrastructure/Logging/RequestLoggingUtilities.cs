using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Events;

namespace ArmaForces.ArmaServerManager.Infrastructure.Logging
{
    internal static class RequestLoggingUtilities
    {
        private const string ApiUrlPart = "api/";

        public static Func<HttpContext, double, Exception?, LogEventLevel> LogOnTraceUnlessErrorOrApiRequest()
        {
            return (httpContext, requestTimeInMs, exception) => exception != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : IsApi(httpContext)
                        ? LogEventLevel.Information
                        : LogEventLevel.Verbose;
        }

        private static bool IsApi(HttpContext httpContext) => httpContext.Request.GetEncodedUrl().Contains(ApiUrlPart);
    }
}