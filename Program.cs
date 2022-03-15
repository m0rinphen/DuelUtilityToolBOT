using System;
using System.Security.Cryptography;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DuelUtilityToolBOT
{
    public class Program
    {
        public static DiscordSocketClient Client;
        public static CommandService Commands;
        public static IServiceProvider Provider;
        public async Task MainAsync()
        {
            Provider = new ServiceCollection().BuildServiceProvider();
            Commands = new CommandService();
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);

            Client = new DiscordSocketClient();
            Client.MessageReceived += CommandRecieved;
            Client.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };

            await Client.LoginAsync(TokenType.Bot, "BOTのトークン");
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task CommandRecieved(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);

            if(message?.Author.IsBot ?? true)
            {
                return;
            }

            var context = new CommandContext(Client, message);
            if ((message.Channel.Name == "general" || message.Channel.Name == "bot-sandbox") && message.Content.Contains("/dice"))
            {
                var numString = message.Content.Replace("/dice ", "");
                int numInt;
                if(int.TryParse(numString, out numInt))
                {
                    for(int ic = 0; ic < numInt; ic++)
                    {
                        var rnd = GenerateRandomNumber(1, 7);
                        await message.Channel.SendMessageAsync($"ダイスロールの結果：「{rnd}」");
                    }
                }
                else
                {
                    var rnd = GenerateRandomNumber(1, 7);
                    await message.Channel.SendMessageAsync($"ダイスロールの結果：「{rnd}」");
                }
            }

            else if(message.Channel.Name == "general" && message.Content.Contains("/coin"))
            {
                var numString = message.Content.Replace("/coin ", "");
                int numInt;
                if(int.TryParse(numString, out numInt))
                {
                    for(int ic = 0; ic < numInt; ic++)
                    {
                        var coinTossResult = GenerateRandomNumber(0, 2) == 1 ? "表" : "裏";
                        await message.Channel.SendMessageAsync($"コイントスの結果：「{coinTossResult}」");
                    }
                }
                else
                {
                    var coinTossResult = GenerateRandomNumber(0, 2) == 1 ? "表" : "裏";
                    await message.Channel.SendMessageAsync($"コイントスの結果：「{coinTossResult}」");
                }
            }
        }

        public int GenerateRandomNumber(int min, int max)
        {
            byte[] bs = new byte[4];
            RNGCryptoServiceProvider rng =new RNGCryptoServiceProvider();
            rng.GetBytes(bs);
            rng.Dispose();
            Random rnd = new Random(System.BitConverter.ToInt32(bs, 0));
            return rnd.Next(min, max);
        }
    }
}