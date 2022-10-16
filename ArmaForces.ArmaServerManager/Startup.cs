using System;
using System.Text.Json.Serialization;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Features.Configuration;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Jobs.Filters;
using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence;
using ArmaForces.ArmaServerManager.Features.Missions.DependencyInjection;
using ArmaForces.ArmaServerManager.Features.Mods.DependencyInjection;
using ArmaForces.ArmaServerManager.Features.Servers;
using ArmaForces.ArmaServerManager.Features.Servers.Providers;
using ArmaForces.ArmaServerManager.Features.Status;
using ArmaForces.ArmaServerManager.Infrastructure;
using ArmaForces.ArmaServerManager.Infrastructure.Authentication;
using ArmaForces.ArmaServerManager.Infrastructure.Converters;
using ArmaForces.ArmaServerManager.Infrastructure.Logging;
using ArmaForces.ArmaServerManager.Services;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using HangfireJobStorage = Hangfire.JobStorage;

namespace ArmaForces.ArmaServerManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }
        
        private OpenApiInfo OpenApiConfiguration { get; } = new OpenApiInfo()
        {
            Title = "ArmaForces ArmaServerManager API",
            Description = "API for Arma 3 server management.",
            Version = "v3",
            Contact = new OpenApiContact
            {
                Name = "ArmaForces",
                Url = new Uri("https://armaforces.com")
            }
        };

        private IConfiguration Configuration { get; }
        
        private IWebHostEnvironment WebHostEnvironment { get; }

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

            .AddSingleton(WebHostEnvironment)
            .AddHostedService<StartupService>()

            // Job services
            .AddSingleton<MaintenanceService>()
            .AddSingleton<IMissionPreparationService, MissionPreparationService>()
            .AddSingleton<IModsUpdateService, ModsUpdateService>()
            .AddSingleton<IServerStartupService, ServerStartupService>()

            // Arma Server
            .AddArmaServer()
            
            // Documentation
            .AddDocumentation(OpenApiConfiguration)
            
            // Mods
            .AddMods()

            // Mission
            .AddMissionsApiClient()

            // Server
            .AddSingleton<ServerProviderFactory>()
            .AddSingleton<IServerProvider>(x => x.GetRequiredService<ServerProviderFactory>()
                .CreateServerProviderAsync(x).Result)
            .AddSingleton<IServerConfigurationLogic, ServerConfigurationLogic>()
            .AddSingleton<IServerCommandLogic, ServerCommandLogic>()
            .AddSingleton<IServerQueryLogic, ServerQueryLogic>()
            
            // Status
            .AddSingleton<IStatusProvider, StatusProvider>()

            // Hangfire
            .AddSingleton<IJobsScheduler, JobsScheduler>()
            .AddSingleton<IJobsService, JobsService>()
            .AddSingleton<IHangfireBackgroundJobClientWrapper, HangfireBackgroundJobClientWrapper>()
            .AddSingleton<IJobsRepository, JobsRepository>()
            .AddSingleton<IJobsDataAccess, JobsDataAccess>()
            
            .AddSingleton<IBackgroundJobClient, BackgroundJobClient>()
            .AddSingleton(_ => HangfireJobStorage.Current.GetMonitoringApi())
            .AddSingleton(_ => HangfireJobStorage.Current.GetConnection())
            .AddSingleton(_ => HangfireDbContext.Instance(Configuration.GetConnectionString("HangfireConnection")))

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

            app.UseSerilogRequestLogging(opt =>
            {
                opt.GetLevel = RequestLoggingUtilities.LogOnTraceUnlessErrorOrApiRequest();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.AddDocumentation(OpenApiConfiguration);

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
