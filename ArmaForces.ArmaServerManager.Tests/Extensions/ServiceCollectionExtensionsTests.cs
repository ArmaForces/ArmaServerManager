using ArmaForces.ArmaServerManager.Tests.Helpers.Dummy;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Extensions
{
    [Trait("Category", "Unit")]
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOrReplaceSingleton_ServiceNotRegistered_AddsNewService()
        {
            var newService = new DummyClass();
            var serviceProvider = new ServiceCollection()
                .AddOrReplaceSingleton(newService)
                .BuildServiceProvider();

            var requiredService = serviceProvider.GetRequiredService<DummyClass>();
            requiredService.Should().BeSameAs(newService);
        }

        [Fact]
        public void AddOrReplaceSingleton_ServiceAlreadyRegistered_ReplacesWithNewService()
        {
            var oldService = new DummyClass();
            var newService = new DummyClass();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(oldService)
                .AddOrReplaceSingleton(newService)
                .BuildServiceProvider();

            var requiredService = serviceProvider.GetRequiredService<DummyClass>();
            using (new AssertionScope())
            {
                requiredService.Should().BeSameAs(newService);
                requiredService.Should().NotBeSameAs(oldService);
            }
        }
    }
}
