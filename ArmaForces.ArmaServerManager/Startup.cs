using System;
using System.Text.Json.Serialization;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers;
using ArmaForces.Arma.Server.Providers.Configuration;
using ArmaForces.Arma.Server.Providers.Keys;
using ArmaForces.ArmaServerManager.Features.Configuration;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Steam;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Providers;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(
                config =>
                {
                    config.AddSimpleConsole(
                        console =>
                        {
                            console.TimestampFormat = "s";
                        });
                });

            services.AddRazorPages();

            // Add REST API Controller
            services.AddControllers();

            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseLiteDbStorage(Configuration.GetConnectionString("HangfireConnection")))

            // Add the processing server as IHostedService
            .AddHangfireServer(ConfigureHangfireServer())

            .AddHostedService<StartupService>()

            // Job services
            .AddSingleton<MaintenanceService>()
            .AddSingleton<IMissionPreparationService, MissionPreparationService>()
            .AddSingleton<IModsUpdateService, ModsUpdateService>()
            .AddSingleton<IServerStartupService, ServerStartupService>()

            .AddSingleton<ISettings>(Settings.LoadSettings)

            // Mods
            .AddSingleton<IModsCache, ModsCache>()
            .AddSingleton<IModsManager, ModsManager>()
            .AddSingleton<IModDirectoryFinder, ModDirectoryFinder>()
            .AddSingleton<IApiModsetClient, ApiModsetClient>()
            .AddSingleton<ISteamClient, SteamClient>()
            .AddSingleton<IContentDownloader, ContentDownloader>()
            .AddSingleton<IContentVerifier, ContentVerifier>()
            .AddSingleton<IModsetProvider, ModsetProvider>()

            // Mission
            .AddSingleton<IApiMissionsClient, ApiMissionsClient>()

            // Configuration
            .AddSingleton<ConfigFileCreator>()
            .AddSingleton<ConfigReplacer>()

            // Keys
            .AddSingleton<IKeysProvider, KeysProvider>()

            // Process
            .AddSingleton<IArmaProcessDiscoverer, ArmaProcessDiscoverer>()
            .AddSingleton<IArmaProcessFactory, ArmaProcessFactory>()
            .AddSingleton<IArmaProcessManager, ArmaProcessManager>()

            // Server
            .AddSingleton<IServerProvider, ServerProvider>()
            .AddSingleton<IServerConfigurationProvider, ServerConfigurationProvider>()
            .AddSingleton<IServerConfigurationLogic, ServerConfigurationLogic>()
            .AddSingleton<IDedicatedServerFactory, DedicatedServerFactory>()
            .AddSingleton<IServerBuilder, ServerBuilder>()
            .AddSingleton<IServerBuilderFactory, ServerBuilderFactory>()
            .AddSingleton<IParametersExtractor, ParametersExtractor>()

            // Hangfire
            .AddSingleton<IHangfireBackgroundJobClient, HangfireBackgroundJobClient>()
            .AddSingleton<IHangfireJobStorage, HangfireJobStorage>()
            .AddSingleton<IHangfireManager, HangfireManager>()
            
            // Security
            .AddSingleton<IApiKeyProvider, ApiKeyProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                IgnoreAntiforgeryToken = true
            });

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHangfireDashboard();
                endpoints.MapControllers();
            });
        }

        private static Action<BackgroundJobServerOptions> ConfigureHangfireServer()
            => backgroundJobServerOptions =>
            {
                backgroundJobServerOptions.WorkerCount = 1;
            };
    }
}
