using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using ChihiroBot.Modules.Gif;
using ChihiroBot.Modules.N_des;
using ChihiroBot.Modules.StarlightStage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChihiroBot.Modules.Random
{
    internal class RandomModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private System.Random x = new System.Random();
        private GifModule gif = new GifModule();
        private N_desModule nudes = new N_desModule();
        private StarlightStageModule ss = new StarlightStageModule();
        private string dropboxFolderPath;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            FindDropboxFolder();

            manager.CreateCommands("", group =>
            {
                group.CreateCommand("hi")
                       .Description("Say hi!")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           switch (x.Next(2))
                           {
                               case 0:
                                   await e.Channel.SendMessage($"hi :)");
                                   break;
                               case 1:
                                   await e.Channel.SendMessage($"hello :D");
                                   break;
                               default:
                                   await e.Channel.SendMessage($"hi :)");
                                   break;
                           }

                       });
                group.CreateCommand("backup")
                       .Parameter("file(s)", ParameterType.Required)
                       .Description("Backup json config files to the Dropbox folder")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{Backup(e)}");

                       });
                group.CreateCommand("restore")
                       .Parameter("file(s)", ParameterType.Required)
                       .Description("Restore json config files from the Dropbox folder")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{Restore(e)}");

                       });
                group.CreateCommand("getlogs")
                       .Parameter("count", ParameterType.Optional)
                       .Description("Gets UmiBot [count] log messages via private message")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(e =>
                       {
                           PmLogs(e);
                       });
                group.CreateCommand("bad pull")
                       .Description("For when you get a bad pull...")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           switch (x.Next(3))
                           {
                               case 0:
                                   await e.Channel.SendMessage($"http://i.imgur.com/OpQjMan.gif");
                                   break;
                               case 1:
                                   await e.Channel.SendMessage($"http://i.imgur.com/5hfeKVU.gif");
                                   break;
                               case 2:
                                   await e.Channel.SendMessage($"http://i.imgur.com/zBnDDQ0.gif");
                                   break;
                               default:
                                   await e.Channel.SendMessage($"http://i.imgur.com/zBnDDQ0.gif");
                                   break;
                           }
                       });
                group.CreateCommand("jew")
                       .Description("A picture of a Jew")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"http://i.imgur.com/DUJ0IG5.png");
                       });
                group.CreateCommand("basic")
                       .Description("The most basic things around")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"http://i.imgur.com/1XCRsrm.jpg");
                       });
                group.CreateCommand("sleep")
                       .Parameter("Text", ParameterType.Optional)
                       .Description("Random 006 for sleep")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{GetSleep(e)}");
                       });
            });
        }

        private void FindDropboxFolder()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbPath = System.IO.Path.Combine(appDataPath, "Dropbox\\host.db");
            string[] lines = System.IO.File.ReadAllLines(dbPath);
            byte[] dbBase64Text = Convert.FromBase64String(lines[1]);
            dropboxFolderPath = System.Text.ASCIIEncoding.ASCII.GetString(dbBase64Text);
        }

        private string Backup(CommandEventArgs e)
        {
            string a, b, c = "";
            switch (e.Args[0])
            {
                case "all":
                    a = Copy(GifModule.imagesJsonFilePathFull, $"{dropboxFolderPath}\\config\\images.json");
                    b = Copy(N_desModule.filePathFull, $"{dropboxFolderPath}\\config\\n_des_images.json");
                    c = Copy(StarlightStageModule.filePathFull, $"{dropboxFolderPath}\\config\\UmiBot.json");
                    if (String.Equals(a, b) && String.Equals(b, c))
                    {
                        return "all ok";
                    }
                    else
                    {
                        return "something failed";
                    }
                case "gif":
                    a = Copy(GifModule.imagesJsonFilePathFull, $"{dropboxFolderPath}\\config\\images.json");
                    return $"{a}";
                case "nudes":
                    b = Copy(N_desModule.filePathFull, $"{dropboxFolderPath}\\config\\n_des_images.json");
                    return $"{b}";
                case "umibot":
                    c = Copy(StarlightStageModule.filePathFull, $"{dropboxFolderPath}\\config\\UmiBot.json");
                    return $"{c}";
                default:
                    a = Copy(GifModule.imagesJsonFilePathFull, $"{dropboxFolderPath}\\config\\images.json");
                    b = Copy(N_desModule.filePathFull, $"{dropboxFolderPath}\\config\\n_des_images.json");
                    c = Copy(StarlightStageModule.filePathFull, $"{dropboxFolderPath}\\config\\UmiBot.json");
                    if (String.Equals(a, b) && String.Equals(b, c))
                    {
                        return "all ok";
                    }
                    else
                    {
                        return "something failed";
                    }
            }
        }

        private string Restore(CommandEventArgs e)
        {
            string a, b, c = "";
            switch (e.Args[0])
            {
                case "all":
                    a = Copy($"{dropboxFolderPath}\\config\\images.json", GifModule.imagesJsonFilePathFull);
                    b = Copy($"{dropboxFolderPath}\\config\\n_des_images.json", N_desModule.filePathFull);
                    c = Copy($"{dropboxFolderPath}\\config\\UmiBot.json", StarlightStageModule.filePathFull);
                    if (String.Equals(a, b) && String.Equals(b, c))
                    {
                        return "all ok";
                    }
                    else
                    {
                        return "something failed";
                    }
                case "gif":
                    a = Copy($"{dropboxFolderPath}\\config\\images.json", GifModule.imagesJsonFilePathFull);
                    return $"{a}";
                case "nudes":
                    b = Copy($"{dropboxFolderPath}\\config\\n_des_images.json", N_desModule.filePathFull);
                    return $"{b}";
                case "umibot":
                    c = Copy($"{dropboxFolderPath}\\config\\UmiBot.json", StarlightStageModule.filePathFull);
                    return $"{c}";
                default:
                    a = Copy($"{dropboxFolderPath}\\config\\images.json", GifModule.imagesJsonFilePathFull);
                    b = Copy($"{dropboxFolderPath}\\config\\n_des_images.json", N_desModule.filePathFull);
                    c = Copy($"{dropboxFolderPath}\\config\\UmiBot.json", StarlightStageModule.filePathFull);
                    if (String.Equals(a, b) && String.Equals(b, c))
                    {
                        return "all ok";
                    }
                    else
                    {
                        return "something failed";
                    }
            }
        }

        private string Copy(string source, string destination)
        {
            try
            {
                File.Copy(source, destination, true);
                return "ok";
            }
            catch (Exception e)
            {
                return $"{e.InnerException.ToString()}";
            }
        }

        private string GetSleep(CommandEventArgs e)
        {
            string sleepUrl = "";
            string[] imageUrl = {
                "http://i.imgur.com/RKlZlOQ.jpg",
                "http://i.imgur.com/RG4umjv.png",
                "http://i.imgur.com/qcCNi62.jpg",
                "http://i.imgur.com/mZm7u5Y.png",
                "http://i.imgur.com/tEL3LID.jpg",
                "http://i.imgur.com/I5Wc8JB.jpg"
            };

            int maxImageUrls = 6;
            try
            {
                int request = Int32.Parse(e.Args[0]);

                if (request >= 0 && request < maxImageUrls)
                {
                    sleepUrl = imageUrl[request];
                }
                else
                {
                    sleepUrl = imageUrl[x.Next(0, maxImageUrls)];
                }
            }
            catch (Exception)
            {
                sleepUrl = imageUrl[x.Next(0, maxImageUrls)];
            }

            return sleepUrl;
        }

        private async void PmLogs(CommandEventArgs e)
        {
            int messages = 5;
            try
            {
                messages = int.Parse(e.Args[0]);
            }
            catch (Exception)
            {

            }

            Discord.Channel chan = await _client.CreatePrivateChannel(GlobalSettings.Users.DevId);
            string[] logdata = File.ReadAllLines(Path.GetFullPath(@".\config\UmiBot.log"));

            for (int i = logdata.Count() - messages; i < logdata.Count(); i++)
            {
                await chan.SendMessage(logdata[i]);
            }
            await chan.SendMessage("Logs sent!");
        }
    }
}
