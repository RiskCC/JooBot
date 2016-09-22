using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChihiroBot.Modules.N_des
{
    internal class N_desModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private static string filePath = "./config/n_des_images.json";
        public static string filePathFull = Path.GetFullPath(filePath);

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            LoadImagesJson(filePath);

            manager.CreateCommands("", group =>
            {
                group.CreateCommand("update nudes")
                       .Description("Updates with lastest json")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           LoadImagesJson(filePath);
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"nudes json updated!");
                       });
                group.CreateCommand("nudes")
                       .Parameter("name", ParameterType.Required)
                       .Description("Get a nudes")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{GetImage(e)}");
                       });
                group.CreateCommand("addnudes")
                       .Parameter("name", ParameterType.Required)
                       .Parameter("url", ParameterType.Required)
                       .Description("Add a nudes")
                       .MinPermissions((int)PermissionLevel.UserPlus)
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{AddImage(e)}");
                       });
                group.CreateCommand("Kippei nudes")
                       .Description("Random 001 for Jhazat")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"(๑°o°๑) instead, please use: nudes Kippei");
                       });
                group.CreateCommand("Jhazat nudes")
                       .Description("Random 002 for Jhazat")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"(๑°o°๑) instead, please use: nudes Jhazat");
                       });
                group.CreateCommand("Coco nudes")
                       .Description("Random 003 for Owl")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"(๑°o°๑) instead, please use: nudes Coco");
                       });
                group.CreateCommand("Zeraek nudes")
                       .Description("Random 004 for Zeraek")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"(๑°o°๑) instead, please use: nudes Zeraek");
                       });
                group.CreateCommand("Neal nudes")
                       .Description("Random 005 for Neal")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"(๑°o°๑) instead, please use: nudes Neal");
                       });
                group.CreateCommand("Madi nudes")
                       .Description("Random 006 for Madi")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"(๑°o°๑) instead, please use: nudes Madi");
                       });
            });
        }
        private string GetImage(CommandEventArgs e)
        {
            string result = "";

            try
            {
                string name = e.Args[0];

                if (String.IsNullOrWhiteSpace(name))
                {
                    result = "whose nudes do you want?";
                }
                else
                {
                    int index = nudes.FindIndex(s => string.Equals(s.name, name, StringComparison.OrdinalIgnoreCase));
                    result = nudes[index].url;
                }
            }
            catch (Exception)
            {
                result = "not found; try again (๑◕︵◕๑)";
            }

            return result;
        }
        private void WriteJson(string filePath)
        {
            string json = JsonConvert.SerializeObject(nudes, Formatting.Indented);
            System.IO.File.WriteAllText($"{filePath}", json);
        }

        private string AddImage(CommandEventArgs e)
        {
            try
            {
                string name = e.Args[0];
                string url = e.Args[1];

                Nudes penguin = new Nudes()
                {
                    name = name,
                    url = url
                };

                int index = nudes.FindLastIndex(s => String.Equals(s.name, penguin.name, StringComparison.OrdinalIgnoreCase));

                if (index != -1)
                {
                    nudes[index] = penguin;
                }
                else
                {
                    nudes.Add(penguin);
                }

                WriteJson(filePath);
                return $"Add successful for {name}";
            }
            catch (Exception)
            {
                return $"Unsuccessful add.";
            }
        }

        private void LoadImagesJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath} is missing.");
            nudes = JsonConvert.DeserializeObject<List<Nudes>>(File.ReadAllText(filePath));
        }

        internal class Nudes
        {
            public string name;
            public string url;
        }
        private List<Nudes> nudes = new List<Nudes>();
    }
}
