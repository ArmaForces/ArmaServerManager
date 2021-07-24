using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArmaForces.ArmaServerManager.Tests.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrReplaceSingleton<TService>(this IServiceCollection services, TService service)
            where TService : class
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(TService), service);
            
            return services.Contains(serviceDescriptor)
                ? services.Replace(serviceDescriptor)
                : services.AddSingleton(service);
        }
    }
}
