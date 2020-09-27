using Blockcore.Settings;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TipBot;
using TipBot.Database;
using TipBot.Helpers;
using TipBot.Logic;
using TipBot.Logic.NodeIntegrations;
using TipBot.Services;

namespace Blockcore.TipBot.Dashboard
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
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // services.AddSingleton<TipBot.Logic.TipBot>();
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<CommandsManager>();
            services.AddSingleton<QuizExpiryChecker>();
            services.AddSingleton<FatalErrorNotifier>();
            services.AddSingleton<IContextFactory, ContextFactory>();
            services.AddSingleton<DiscordConnectionKeepAlive>();
            services.AddSingleton<MessagesHelper>();
            services.AddSingleton<INodeIntegration, BlockCoreNodeIntegration>();

            // services.AddHostedService<Worker>();

            services.Configure<TipBotSettings>(Configuration.GetSection("TipBot"));
            services.Configure<ChainSettings>(Configuration.GetSection("Chain"));
            services.Configure<NetworkSettings>(Configuration.GetSection("Network"));
            services.Configure<IndexerSettings>(Configuration.GetSection("Indexer"));
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
            }

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
