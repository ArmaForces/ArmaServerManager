using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArmaForces.ArmaServerManager.Tests.Features.Missions.TestingHelpers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // This is used as IClassFixture and instantiated by xUnit.
    public class MissionsTestApiFixture : IDisposable
    {
        private const int Port = 43423;
        private readonly SocketsHttpHandler _socketsHttpHandler;
        private readonly IHost _host;

        public HttpClient HttpClient { get; }

        public MissionsTestApiFixture()
        {
            _socketsHttpHandler = new SocketsHttpHandler();
            HttpClient = new HttpClient(_socketsHttpHandler)
            {
                BaseAddress = new Uri($"http://localhost:{Port}")
            };
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(ConfigureWebBuilder())
                .Build();

            _host.Start();
        }

        private static Action<IWebHostBuilder> ConfigureWebBuilder() => webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.UseKestrel(x => x.ListenLocalhost(Port));
        };

        public void Dispose()
        {
            HttpClient.Dispose();
            _socketsHttpHandler.Dispose();
            _host.Dispose();
        }

        private class Startup
        {
            public void ConfigureServices(IServiceCollection services) => services
                .AddMvcCore(options => options.EnableEndpointRouting = false)
                .AddJsonOptions(
                    x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            public void Configure(IApplicationBuilder app) => app.UseMvc();
        }
    }
}
