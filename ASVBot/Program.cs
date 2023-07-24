﻿using ASVBot.Commands;
using ASVBot.Config;
using ASVBot.Data;
using ASVPack;
using ASVPack.Models;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;
using System.Timers;

namespace ASVBot
{
    internal class Program
    {
        public static ContentContainer arkPack = new ContentContainer();
        private static BotConfig config = new BotConfig();
        private static List<IClassMap> classMaps = new List<IClassMap>();
        private static ContentContainerGraphics graphicsContainer = null;

        static async Task Main(string[] args)
        {
            var configFilename = Path.Combine(AppContext.BaseDirectory, "botconfig.json");
            if(!File.Exists(configFilename))
            {

                return;
            }


            classMaps = new List<IClassMap>();

            var creatureMapConfigFilename = Path.Combine(AppContext.BaseDirectory, "creaturemap.json");
            if (File.Exists(creatureMapConfigFilename))
            {


                string jsonFileContent = File.ReadAllText(creatureMapConfigFilename);

                JObject dinoFile = JObject.Parse(jsonFileContent);
                JArray dinoList = (JArray)dinoFile.GetValue("creatures");
                if(dinoList!= null)
                {
                    foreach (JObject dinoObject in dinoList)
                    {
                        if (dinoObject != null)
                        {
                            IClassMap dino = new ClassMap();
                            dino.ClassName = dinoObject.Value<string>("ClassName")??"";
                            dino.FriendlyName = dinoObject.Value<string>("FriendlyName")??"";
                            classMaps.Add(dino);
                        }
                    }
                }               

            }

            var structureMapConfigFilename = Path.Combine(AppContext.BaseDirectory, "structuremap.json");
            if (File.Exists(structureMapConfigFilename))
            {

                string jsonFileContent = File.ReadAllText(structureMapConfigFilename);

                JObject structureFile = JObject.Parse(jsonFileContent);
                JArray structureList = (JArray)structureFile.GetValue("structures");
                if (structureList != null)
                {
                    foreach (JObject structureObject in structureList)
                    {
                        if (structureObject != null)
                        {
                            IClassMap structureMap = new ClassMap();
                            structureMap.ClassName = structureObject.Value<string>("ClassName") ?? "";
                            structureMap.FriendlyName = structureObject.Value<string>("FriendlyName") ?? "";
                            classMaps.Add(structureMap);
                        }
                    }
                }

            }

            var itemMapConfigFilename = Path.Combine(AppContext.BaseDirectory, "itemmap.json");
            if (File.Exists(itemMapConfigFilename))
            {

                string jsonFileContent = File.ReadAllText(itemMapConfigFilename);

                JObject itemFile = JObject.Parse(jsonFileContent);
                JArray itemList = (JArray)itemFile.GetValue("items");
                if (itemList != null)
                {
                    foreach (JObject itemObject in itemList)
                    {
                        if (itemObject != null)
                        {
                            IClassMap itemMap = new ClassMap();
                            itemMap.ClassName = itemObject.Value<string>("ClassName") ?? "";
                            itemMap.FriendlyName = itemObject.Value<string>("FriendlyName") ?? "";
                            classMaps.Add(itemMap);
                        }
                    }
                }

            }


            var configFileData = File.ReadAllText(configFilename);
            config = JsonConvert.DeserializeObject<BotConfig>(configFileData)??new BotConfig();

            if(!File.Exists(config.ArkSaveFile))
            {
                return;
            }

            arkPack.LoadSaveGame(config.ArkSaveFile, string.Empty, config.ArkClusterFolder);
            System.Timers.Timer reloadTimer = new System.Timers.Timer(60 * config.AutoReloadTimeMinutes);
            reloadTimer.Elapsed += (sender,args) =>
            {
                arkPack.Reload();
            };

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = config.DiscordBotToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            graphicsContainer = new ContentContainerGraphics(arkPack, new ContentMapPack());

            var slash = discord.UseSlashCommands(new SlashCommandsConfiguration()
            {
                

                Services = new ServiceCollection()
                            .AddSingleton<BotConfig>(config)
                            .AddSingleton<ContentContainerGraphics>(graphicsContainer)
                            .AddSingleton<IContentContainer>(arkPack)         
                            .AddSingleton<IDiscordPlayerManager>(new DiscordPlayerManager())                            
                            .AddSingleton<List<IClassMap>>(classMaps)

                            .BuildServiceProvider()
            });

            RegisterCommands(slash);
            
            await discord.ConnectAsync();
          
            await Task.Delay(-1);

        }


        private static void RegisterCommands(SlashCommandsExtension slash)
        {
            if (config.DiscordServerId != 0)
            {
                slash.RegisterCommands<AdminCommands>((ulong)config.DiscordServerId);               
                slash.RegisterCommands<PlayerCommands>((ulong)config.DiscordServerId);
                slash.RegisterCommands<GeneralCommands>((ulong)config.DiscordServerId);

            }
            else
            {
                slash.RegisterCommands<AdminCommands>();
                slash.RegisterCommands<PlayerCommands>();
                slash.RegisterCommands<GeneralCommands>();
            }
        }
    }
}