using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TipBot.Helpers;
using TipBot.Services;

namespace TipBot
{
    /*
        TODO:
        1) Connect to DB. SQL + EntityFramework?
        2) Outline all methods we want to implement.
        3) Add logging
    */

    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        public async Task MainAsync(string[] args)
        {
            IServiceProvider services = this.ConfigureServices();

            var settings = services.GetRequiredService<Settings>();
            settings.Initialize(new TextFileConfiguration(args));

            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += this.LogAsync;
            services.GetRequiredService<CommandService>().Log += this.LogAsync;

            await client.LoginAsync(TokenType.Bot, settings.BotToken);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PictureService>()
                .AddSingleton<Settings>()
                .BuildServiceProvider();
        }
    }
}
