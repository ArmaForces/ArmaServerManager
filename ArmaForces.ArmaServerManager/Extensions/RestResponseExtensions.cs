using RestSharp;

namespace ArmaForces.ArmaServerManager.Extensions
{
    internal static class RestResponseExtensions
    {
        public static string GetErrorString<T>(this IRestResponse<T> response)
        {
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                return response.Content;
            }

            return response.ErrorMessage + "|" + response.ErrorException;
        }
    }
}
