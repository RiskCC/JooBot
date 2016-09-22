using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChihiroBot.Modules.Timer
{
    internal class TimerModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private System.Random x = new System.Random();
        private static string timerJsonFilePath = "./config/timer.json";
        public static string timerJsonFilePathFull = Path.GetFullPath(timerJsonFilePath);
        private JToken timerJson, times;
        private JObject groups, items;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            LoadJson(timerJsonFilePath);
            //Test();

            manager.CreateCommands("", group =>
            {
                group.CreateCommand("update timer")
                       .Description("Updates with lastest json")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           LoadJson(timerJsonFilePath);
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"gif json updated!");

                       });
            });
            manager.CreateCommands("timer", group =>
            {
                group.CreateCommand("add")
                       .Parameter("group", ParameterType.Required)
                       .Parameter("event", ParameterType.Required)
                       .Parameter("time", ParameterType.Required)
                       .Description("Adds a time to a group event")
                       .MinPermissions((int)PermissionLevel.UserPlus)
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{AddTimer(e)}");
                       });
                group.CreateCommand("ss")
                       .Description("Gets remaining SS event time formatted in days:hours:minutes:seconds")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"Time remaining: {GetTimer("ss", "event")}");
                       });
                group.CreateCommand("sifjp")
                       .Description("Gets remaining SIF JP event time formatted in days:hours:minutes:seconds")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"Time remaining: {GetTimer("sifjp", "event")}");
                       });
                group.CreateCommand("sifen")
                       .Description("Gets remaining SIF EN event time formatted in days:hours:minutes:seconds")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"Time remaining: {GetTimer("sifen", "event")}");
                       });
            });
        }

        private void Write(object content, string filePath)
        {
            string json = JsonConvert.SerializeObject(content, Formatting.Indented);
            System.IO.File.WriteAllText($"{filePath}", json);
        }

        private void LoadJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath} is missing.");
            timerJson = JObject.Parse(File.ReadAllText(filePath));
        }

        private string AddTimer(CommandEventArgs e)
        {
            string group, item, time, result;

            try
            {
                group = e.Args[0];
                item = e.Args[1];
                time = e.Args[2];
            }
            catch
            {
                return $"Couldn't read arguments of {e.Args}";
            }
            try
            {
                groups = timerJson.Value<JObject>();
                items = groups[group] as JObject;
                times = items.GetValue(item);

                times.Replace(time);

                Write(timerJson, timerJsonFilePath);
                result = $"Successfully added {group} {item}!";
            }
            catch
            {
                result = $"Add failed for {group} {item} {time}";
            }

            return result;
        }

        public string GetTimer(CommandEventArgs e)
        {
            string result = "";
            string _group, _item;

            // Read args
            try
            {
                _group = e.Args[0];
            }
            catch
            {
                return "bad arguments";
            }
            try
            {
                _item = e.Args[1];
            }
            catch
            {
                return "bad arguments";
            }

            foreach (JProperty game in timerJson)
            {
                if (game.Name.Equals(_group))
                {
                    foreach (JObject eventtype in game)
                    {
                        foreach (JProperty eventtype_ in eventtype.Properties())
                        {
                            if (eventtype_.Name.Equals(_item))
                            {
                                eventtype_.Value.ToString();
                            }
                        }
                    }
                }
                else
                {
                    result = $"I can't find {_group} in the list D:";
                }
            }

            return result;
        }
        public string GetTimer(string group, string item)
        {
            string result = "";
            string _group, _item;

            LoadJson(timerJsonFilePathFull);

            // Read args
            try
            {
                _group = group;
            }
            catch
            {
                return "bad arguments";
            }
            try
            {
                _item = item;
            }
            catch
            {
                return "bad arguments";
            }

            foreach (JProperty game in timerJson)
            {
                if (game.Name.Equals(_group))
                {
                    foreach (JObject eventtype in game)
                    {
                        foreach (JProperty eventtype_ in eventtype.Properties())
                        {
                            if (eventtype_.Name.Equals(_item))
                            {
                                if (_group.Equals("ss") || _group.Equals("sifjp"))
                                {
                                    return $"{GetTimeRemaining(eventtype_.Value.ToString(), "JST")}.";
                                }
                                else if (_group.Equals("sifen"))
                                {
                                    return $"{GetTimeRemaining(eventtype_.Value.ToString(), "UTC")}.";
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = $"I can't find {_group} in the list D:";
                }
            }

            return result;
        }

        private string GetTimeRemaining(string endtime, string timezone)
        {
            if (String.IsNullOrWhiteSpace(endtime))
            {
                return "no event time set";
            }
            else if (timezone.Equals("UTC"))
            {
                if (DateTime.Parse(endtime) < DateTime.UtcNow)
                {
                    return "event finished";
                }
                else
                {
                    return (string.Format("{0:dd\\:hh\\:mm\\:ss}", DateTime.Parse(endtime) - DateTime.UtcNow));
                }
            }
            else
            {
                if (DateTime.Parse(endtime) < TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")))
                {
                    return "event finished";
                }
                else
                {
                    return (string.Format("{0:dd\\:hh\\:mm\\:ss}", DateTime.Parse(endtime) - TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"))));
                }
            }

        }


    }
}
