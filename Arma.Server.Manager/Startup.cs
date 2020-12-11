using Arma.Server.Config;
using Arma.Server.Manager.Clients.Missions;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Manager.Features.Configuration;
using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Features.Hangfire.Helpers;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Manager.Features.Steam;
using Arma.Server.Manager.Features.Steam.Content;
using Arma.Server.Manager.Infrastructure.Authentication;
using Arma.Server.Manager.Providers;
using Arma.Server.Manager.Providers.Server;
using Arma.Server.Manager.Services;
using Arma.Server.Providers.Configuration;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager
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
            services.AddRazorPages();

            // Add REST API Controller
            services.AddControllers();

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseLiteDbStorage(Configuration.GetConnectionString("HangfireConnection")));

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            services.AddHostedService<StartupService>();

            services.AddSingleton<ISettings>(Settings.LoadSettings);
            services.AddSingleton<IModsCache>(ModsCache.CreateModsCache);
            services.AddSingleton<IModsManager, ModsManager>();
            services.AddSingleton<IApiModsetClient>(ApiModsetClient.CreateApiModsetClient);
            services.AddSingleton<IApiMissionsClient, ApiMissionsClient>();
            services.AddSingleton<ISteamClient, SteamClient>();
            services.AddSingleton<IContentDownloader>(ContentDownloader.CreateContentDownloader);
            services.AddSingleton<IContentVerifier>(ContentVerifier.CreateContentVerifier);
            services.AddSingleton<IModsetProvider, ModsetProvider>();
            services.AddSingleton<IServerProvider, ServerProvider>();
            services.AddSingleton<IServerConfigurationProvider>(ServerConfigurationProvider.CreateServerConfigurationProvider);
            services.AddSingleton<IServerConfigurationLogic>(ServerConfigurationLogic.CreateServerConfigurationLogic);
            services.AddSingleton<IModsUpdateService, ModsUpdateService>();
            services.AddSingleton<MaintenanceService>();
            services.AddSingleton<MissionPreparationService>();
            services.AddSingleton<ServerStartupService>();

            services.AddSingleton<IHangfireBackgroundJobClient>(
                HangfireBackgroundJobClient.CreateHangfireBackgroundJobClient);
            services.AddSingleton<IHangfireJobStorage>(HangfireJobStorage.CreateHangfireJobStorage);
            services.AddSingleton<IHangfireManager>(HangfireManager.CreateHangfireManager);

            services.AddSingleton<IApiKeyProvider, ApiKeyProvider>();

            // Add framework services.
            services.AddMvc();
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

            app.UseHangfireDashboard();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHangfireDashboard();
                endpoints.MapControllers();
            });
        }
    }
}
