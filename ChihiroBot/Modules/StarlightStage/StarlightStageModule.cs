using Discord;
using Discord.API.Client;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using ChihiroBot.Modules.Border;
using ChihiroBot.Modules.Timer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;

namespace ChihiroBot.Modules.StarlightStage
{
    internal class StarlightStageModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private static string filePath = "./config/UmiBot.json";
        private static string filePath2 = "./config/tweetinvi.json";
        public static string filePathFull = Path.GetFullPath(filePath);
        public static string filePath2Full = Path.GetFullPath(filePath2);
        private List<Account> accounts = new List<Account>();
        private List<Keys> keys = new List<Keys>();
        private string result, name, id;
        private BorderModule bm = new BorderModule();
        private TimerModule tm = new TimerModule();


        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            LoadJson();
            LoadKeys();
            Auth.SetUserCredentials(keys[0].consumerKey, keys[0].consumerSecret, keys[0].accessToken, keys[0].accessTokenSecret);
            manager.CreateCommands("", group =>
            {
                group.CreateCommand("update ss")
                       .Description("Updates with lastest json")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           LoadJson();
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"ss user json updated!");
                       });
            });
            manager.CreateCommands("ss", group =>
            {
                group.CreateCommand("help")
                       .Description("Returns the usage for SS module.")
                       .Do(e =>
                       {
                           return e.Channel.SendMessage(
                               $"Usage:\n" +
                               "ss get *id*  : get user by account ID #\n" +
                               "ss get *name*  : get user by name if already added with \"ss add\"\n" +
                               "ss add *name* *id*  : associate account ID # with a name\n");
                       });
                group.CreateCommand("get")
                       .Parameter("name|id", ParameterType.Required)
                       .Description("Gets user info based off account ID or name")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           GetMe(e);
                       });
                group.CreateCommand("add")
                       .Parameter("name", ParameterType.Required)
                       .Parameter("id", ParameterType.Required)
                       .Description("Adds user info to the account list")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           AddMe(e);
                       });
                group.CreateCommand("remove")
                       .Parameter("Text", ParameterType.Required)
                       .Description("Removes a user from the account list")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           RemoveMe(e);
                       });
                group.CreateCommand("border")
                       .Description("Alternate method to call border ss")
                       .Do(e =>
                       {
                           bm.GetLastBorderTweet(e, BorderModule.deresute_border);
                       });
                group.CreateCommand("timer")
                       .Description("Alternate method to call timer ss")
                       .Do(async e =>
                       {
                           await e.Channel.SendMessage($"Time remaining: {tm.GetStarlightTimeRemaining("event")}");
                       });
                group.CreateCommand("prediction")
                       .Description("Get last prediction tweet")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           GetLastPredictionTweet(e);
                       });
            });
        }

        private async void GetLastPredictionTweet(CommandEventArgs e)
        {
            var accts = Search.SearchUsers("cindere_border");
            var acct = accts.First();
            var lastTweets = acct.GetUserTimeline(3);
            var lastTweet = "";
            foreach (var tweet in lastTweets)
            {
                if (tweet.Text.Length > 100)
                {
                    lastTweet = tweet.ToString();
                    break;
                }
            }

            if (String.IsNullOrEmpty(lastTweet))
            {
                await e.Channel.SendMessage($"Couldn't find a prediction tweet (๑´╹‸╹`๑)");
            }
            else
            {
                await e.Channel.SendMessage($"{lastTweet.ToString()}");
            }

        }

        private void LoadJson()
        {
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    accounts = JsonConvert.DeserializeObject<List<Account>>(json);
                }
            }
            catch (Exception)
            {
                if (!File.Exists(filePath))
                    File.Create(filePath);
            }
        }

        private void LoadKeys()
        {
            using (StreamReader r = new StreamReader(filePath2))
            {
                string json = r.ReadToEnd();
                keys = JsonConvert.DeserializeObject<List<Keys>>(json);
            }
        }
        private async void AddMe(CommandEventArgs e)
        {
            try
            {
                name = e.Args[0];

                if (Regex.IsMatch(e.Args[1], "^[0-9]{9}$"))
                {
                    id = e.Args[1];
                }
                else
                {
                    throw new SystemException("Invalid ID");
                }

                Write(name, id);

                result = $"{name.ToString()} added";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            await e.Channel.SendMessage($"{result}");
        }

        private async void GetMe(CommandEventArgs e)
        {
            // Discord caches images so we need to force a new image get
            var duck = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            try
            {
                id = Read(e.Args[0]);

                if (String.IsNullOrWhiteSpace(id))
                {
                    throw new SystemException();
                }

                if (String.Compare(e.Args[0], "Risk", true) == 0)
                {
                    Discord.Channel chan = await _client.CreatePrivateChannel(GlobalSettings.Users.DevId);
                    await chan.SendMessage($"{e.User.Name} just used SS GET in {e.Server.Name}");
                }

                result = $"https://deresute.me/{id}/medium.png?{duck}";
            }
            catch (Exception)
            {
                if (Regex.IsMatch(e.Args[0], "^[0-9]{9}$"))
                {
                    result = $"https://deresute.me/{e.Args[0]}/medium.png?{duck}";
                }
                else
                {
                    result = "not found; try again (๑◕︵◕๑)";
                }
            }

            await e.Channel.SendMessage($"{result}");
        }

        private async void RemoveMe(CommandEventArgs e)
        {
            try
            {
                accounts.RemoveAll(a => String.Equals(a.name, e.Args[0], StringComparison.OrdinalIgnoreCase));
                SaveJson();
                await e.Channel.SendMessage($"{e.Args[0]} removed");
            }
            catch (Exception)
            {
                await e.Channel.SendMessage($"{e.Args[0]} not removed");
            }
        }

        private void SaveJson()
        {
            string json = JsonConvert.SerializeObject(accounts.ToArray(), Formatting.Indented);

            System.IO.File.WriteAllText($"{filePath}", json);
        }

        private void Write(string name, string id)
        {
            Account psyduck = new Account()
            {
                name = name,
                id = id
            };

            int index = accounts.FindLastIndex(s => String.Equals(s.name, psyduck.name, StringComparison.OrdinalIgnoreCase));

            if (index != -1)
            {
                accounts[index] = psyduck;
            }
            else
            {
                accounts.Add(psyduck);
            }

            SaveJson();
        }

        private string Read(string name)
        {
            foreach (Account a in accounts)
            {
                if (String.Equals(a.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return a.id;
                }
            }
            return "";
        }
    }

    internal class Keys
    {
        public string consumerKey = null;
        public string consumerSecret = null;
        public string accessToken = null;
        public string accessTokenSecret = null;
    }

    internal class Account
    {
        public string name;
        public string id;
    }
}
