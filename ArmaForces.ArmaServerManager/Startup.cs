using System;
using System.Text.Json.Serialization;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Features.Configuration;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Filters;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Features.Missions.DependencyInjection;
using ArmaForces.ArmaServerManager.Features.Mods.DependencyInjection;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Infrastructure.Converters;
using ArmaForces.ArmaServerManager.Providers.Server;
using ArmaForces.ArmaServerManager.Services;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ArmaForces.ArmaServerManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            // Add REST API Controller
            services.AddControllers();

            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    opt.JsonSerializerOptions.Converters.Add(new DateTimeOffsetConverter());
                    opt.JsonSerializerOptions.IgnoreNullValues = true;
                    opt.JsonSerializerOptions.WriteIndented = true;
                });

            // Add Hangfire services.
            services.AddHangfire((provider, configuration) => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseFilter(provider.GetService<FailOnResultFailureAttribute>())
                .UseLiteDbStorage(Configuration.GetConnectionString("HangfireConnection")))

            // Add the processing server as IHostedService
            .AddHangfireServer(ConfigureHangfireServer())
            .AddTransient<FailOnResultFailureAttribute>()

            .AddHostedService<StartupService>()

            // Job services
            .AddSingleton<MaintenanceService>()
            .AddSingleton<IMissionPreparationService, MissionPreparationService>()
            .AddSingleton<IModsUpdateService, ModsUpdateService>()
            .AddSingleton<IServerStartupService, ServerStartupService>()

            // Arma Server
            .AddArmaServer()
            
            // Mods
            .AddMods()

            // Mission
            .AddMissionsApiClient()

            // Server
            .AddSingleton<IServerProvider, ServerProvider>()
            .AddSingleton<IServerConfigurationLogic, ServerConfigurationLogic>()

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

            app.UseSerilogRequestLogging();

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
