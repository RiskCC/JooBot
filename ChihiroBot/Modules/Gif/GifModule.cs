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

namespace ChihiroBot.Modules.Gif
{
    internal class GifModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private System.Random x = new System.Random();
        private static string imagesJsonFilePath = "./config/images.json";
        public static string imagesJsonFilePathFull = Path.GetFullPath(imagesJsonFilePath);
        private JToken imagesJson;
        private JObject shows, girls;
        private JArray gifs;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            LoadJson(imagesJsonFilePath);
            //Test();
            manager.CreateCommands("", group =>
            {
                group.CreateCommand("update gif")
                       .Description("Updates with lastest json")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           LoadJson(imagesJsonFilePath);
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"gif json updated!");

                       });
                group.CreateCommand("gj")
                       .Parameter("girl number", ParameterType.Multiple)
                       .Description("Gets a gj gif")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{GetGif("gj", e)}");
                       });
                group.CreateCommand("yryr")
                       .Parameter("girl number", ParameterType.Multiple)
                       .Description("Gets a yryr gif")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{GetGif("yryr", e)}");
                       });
                group.CreateCommand("nnb")
                       .Parameter("girl number", ParameterType.Multiple)
                       .Description("Gets a nnb gif")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{GetGif("nnb", e)}");
                       });
            });
            manager.CreateCommands("gif", group =>
            {
                group.CreateCommand("add")
                       .Parameter("list", ParameterType.Required)
                       .Parameter("girl", ParameterType.Required)
                       .Parameter("url", ParameterType.Required)
                       .Description("Adds a gif to a list")
                       .MinPermissions((int)PermissionLevel.UserPlus)
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{AddGif(e)}");
                       });
                group.CreateCommand("get")
                       .Parameter("list girl number", ParameterType.Multiple)
                       .Description("Gets a gif")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"{GetGif(e)}");
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
            imagesJson = JObject.Parse(File.ReadAllText(filePath));
        }

        private void GetGifTest()
        {
            string series = "yryr";
            string girl = "";
            int number = 1;

            foreach (JProperty a in imagesJson)
            {
                Console.WriteLine(a.Name);
                if (a.Name.Equals(series))
                {
                    foreach (JObject b in a)
                    {
                        foreach (JProperty c in b.Properties())
                        {
                            if (String.IsNullOrWhiteSpace(girl))
                            {
                                Console.WriteLine(b.Properties().ElementAt(x.Next(b.Properties().Count())).Name);
                            }

                            if (c.Name.Equals(girl) && number >= 0)
                            {
                                Console.WriteLine(c.Value[number].ToString());
                                foreach (JToken d in c.Value)
                                {
                                    Console.WriteLine(d);
                                }
                            }
                        }
                    }
                }
            }
        }

        private string AddGif(CommandEventArgs e)
        {
            string show, girl, url, result;
            //LoadJson(imagesJsonFilePath);
            try
            {
                show = e.Args[0];
                girl = e.Args[1];
                url = e.Args[2];
            }
            catch
            {
                return $"Couldn't read arguments of {e.Args}";
            }
            try
            {
                shows = imagesJson.Value<JObject>();
                girls = shows[show] as JObject;
                gifs = girls[girl] as JArray;

                gifs.Add(url);
                Write(imagesJson, imagesJsonFilePath);
                result = $"Successfully added {show} {girl}!";
            }
            catch
            {
                result = $"Add failed for {show} {girl} {url}";
            }

            return result;
        }
        private string GetGif(string _show, CommandEventArgs e)
        {
            string result = "";
            string _girl;
            int _number;

            //LoadJson(imagesJsonFilePath);

            try
            {
                _girl = e.Args[0];
            }
            catch
            {
                _girl = "";
            }
            try
            {
                _number = Int32.Parse(e.Args[1]);
            }
            catch
            {
                _number = -1;
            }

            foreach (JProperty show in imagesJson)
            {
                if (String.IsNullOrWhiteSpace(_show))
                {
                    _show = RandomPropertyName(imagesJson.Value<JObject>());
                }

                if (show.Name.Equals(_show))
                {
                    foreach (JObject allGirls in show)
                    {
                        foreach (JProperty girl in allGirls.Properties())
                        {
                            if (String.IsNullOrWhiteSpace(_girl))
                            {
                                // if _girl is not set, pick a random girl from _show
                                _girl = RandomPropertyName(allGirls);
                            }

                            if (girl.Name.Equals(_girl))
                            {
                                if (_number < 0 || _number > girl.Value.Count() || girl.Value.Count() == 1)
                                {
                                    _number = x.Next(girl.Value.Count());
                                }
                                return girl.Value[_number].ToString();
                            }
                        }
                        // if we get here then we've gone through all girls in a show w/o a result
                        // we want to pick a random grill and a random image for that grill
                        JProperty _girl_ = RandomProperty(allGirls);
                        return RandomValue(_girl_);
                    }
                }
                else
                {
                    result = $"I can't find {_show} in the list D:";
                }
            }

            return result;
        }

        private string GetGif(CommandEventArgs e)
        {
            string result = "";
            string _show, _girl;
            int _number;

            // Read args
            try
            {
                _show = e.Args[0];
            }
            catch
            {
                _show = "";
            }
            try
            {
                _girl = e.Args[1];
            }
            catch
            {
                _girl = "";
            }
            try
            {
                _number = Int32.Parse(e.Args[2]);
            }
            catch
            {
                _number = -1;
            }

            foreach (JProperty show in imagesJson)
            {
                if (String.IsNullOrWhiteSpace(_show))
                {
                    _show = RandomPropertyName(imagesJson.Value<JObject>());
                }

                if (show.Name.Equals(_show))
                {
                    foreach (JObject allGirls in show)
                    {
                        foreach (JProperty girl in allGirls.Properties())
                        {
                            if (String.IsNullOrWhiteSpace(_girl))
                            {
                                // if _girl is not set, pick a random girl from _show
                                _girl = RandomPropertyName(allGirls);
                            }

                            if (girl.Name.Equals(_girl))
                            {
                                if (_number < 0 || _number > girl.Value.Count())
                                {
                                    _number = x.Next(girl.Value.Count());
                                }
                                return girl.Value[_number].ToString();
                            }
                        }
                        // if we get here then we've gone through all girls in a show w/o a result
                        // we want to pick a random grill and a random image for that grill
                        JProperty _girl_ = RandomProperty(allGirls);
                        return RandomValue(_girl_);
                    }
                }
                else
                {
                    result = $"I can't find {_show} in the list D:";
                }
            }

            return result;
        }

        private JProperty RandomProperty(JObject obj)
        {
            return obj.Properties().ElementAt(x.Next(obj.Properties().Count()));
        }

        private string RandomPropertyName(JObject obj)
        {
            return obj.Properties().ElementAt(x.Next(obj.Properties().Count())).Name;
        }

        private string RandomValue(JProperty prop)
        {
            return prop.Value[x.Next(prop.Value.Count())].ToString();
        }

    }
}
