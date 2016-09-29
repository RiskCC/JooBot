using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace ChihiroBot.Modules.Timer
{
    internal class TimerModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private System.Random x = new System.Random();

        private static string kiraraUrl = "https://starlight.kirara.ca/api/v1/happening/now";
        private static string sukuTomoCachedDataUrl = "http://schoolido.lu/api/cacheddata/";
        private static string sukuTomoEventsUrl = "http://schoolido.lu/api/events/";

        private Gachas gachasSS;
        private Events eventsSS;
        private SIFCachedEvent sifenCached, sifjpCached;
        private SIFEvent sifenEvent, sifjpEvent;
        private static DateTime noEvent = new DateTime(1993, 1, 26, 0, 0, 0, 0, System.DateTimeKind.Utc);

        private static string JST = "Tokyo Standard Time";
        private static string UTC = "UTC";

        #region Obsolete Fields
        [Obsolete]
        private static string timerJsonFilePath = "./config/timer.json";
        [Obsolete]
        public static string timerJsonFilePathFull = Path.GetFullPath(timerJsonFilePath);
        [Obsolete]
        private JToken timerJson, times;
        [Obsolete]
        private JObject groups, items;
        #endregion

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            //UpdateSIFTimersTest();
            //GetSIFTimeRemainingTest();
            //UpdateStarlightTimersTest();
            //GetStarlightTimeRemainingTest();
            //GetTimeInTest();

            UpdateStarlightTimers();
            UpdateSIFCachedData();
            
            manager.CreateCommands("", group =>
            {
                group.CreateCommand("update timer")
                       .Description("Updates with lastest json")
                       .MinPermissions((int)PermissionLevel.BotOwner)
                       .Do(async e =>
                       {
                           UpdateStarlightTimers();
                           UpdateSIFTimers();
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"timer json updated!");

                       });
            });
            manager.CreateCommands("timer", group =>
            {
                group.CreateCommand("ss")
                       .Description("Gets remaining SS event time formatted in days:hours:minutes:seconds")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"Time remaining: {GetStarlightTimeRemaining("event")}");
                       });
                group.CreateCommand("sifjp")
                       .Description("Gets remaining SIF JP event time formatted in days:hours:minutes:seconds")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"Time remaining: {GetSIFTimeRemaining("sifjp")}");
                       });
                group.CreateCommand("sifen")
                       .Description("Gets remaining SIF EN event time formatted in days:hours:minutes:seconds")
                       .Do(async e =>
                       {
                           await e.Channel.SendIsTyping();
                           await e.Channel.SendMessage($"Time remaining: {GetSIFTimeRemaining("sifen")}");
                       });
            });
        }
        #region TimerModule Methods
        private string GetJson(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private DateTime GetTimeIn(string timezone)
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timezone));
        }

        private void GetTimeInTest()
        {
            DateTime jst = GetTimeIn(JST);
            DateTime utc = GetTimeIn(UTC);
        }

        private DateTime UnixToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        #endregion

        #region Starlight Stage Methods
        public void UpdateStarlightTimers()
        {
            try
            {
                string json = GetJson(kiraraUrl);
                var parsed = JsonConvert.DeserializeObject<SSTimer>(json);

                if (!(parsed.gachas.Count == 0))
                {
                    gachasSS = JsonConvert.DeserializeObject<Gachas>(parsed.gachas[0].ToString());
                }
                if (!(parsed.events.Count == 0))
                {
                    eventsSS = JsonConvert.DeserializeObject<Events>(parsed.events[0].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateStarlightTimersTest()
        {
            UpdateStarlightTimers();
        }

        public string GetStarlightTimeRemaining(string query)
        {
            DateTime end;
            UpdateStarlightTimers();

            if (String.Equals(query, "event"))
            {
                if(!(eventsSS == null))
                {
                    end = UnixToDateTime(eventsSS.end);
                }
                else
                {
                    end = noEvent;
                }
            }
            else if (String.Equals(query, "gacha"))
            {
                if (!(gachasSS == null))
                {
                    end = UnixToDateTime(gachasSS.end);
                }
                else
                {
                    end = noEvent;
                }
            }
            else
            {
                return "no query";
            }

            if (end < DateTime.Now)
            {
                return "finished";
            }
            else
            {
                return (string.Format("{0:dd\\:hh\\:mm\\:ss}", end - DateTime.Now));
            }
        }

        private void GetStarlightTimeRemainingTest()
        {
            var a = GetStarlightTimeRemaining("event");
            var b = GetStarlightTimeRemaining("gacha");
        }
        #endregion

        #region School Idol Festival Methods
        private void UpdateSIFCachedData()
        {
            try
            {
                string json = GetJson(sukuTomoCachedDataUrl);
                var parsed = JsonConvert.DeserializeObject<SIFCachedData>(json);
                sifenCached = parsed.eventEn;
                sifjpCached = parsed.eventJp;

                UpdateSIFTimers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateSIFTimers()
        {
            try
            {
                string json = GetJson(sukuTomoEventsUrl + sifenCached.japanese_name + "/");
                sifenEvent = JsonConvert.DeserializeObject<SIFEvent>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                string json = GetJson(sukuTomoEventsUrl + sifjpCached.japanese_name + "/");
                sifjpEvent = JsonConvert.DeserializeObject<SIFEvent>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateSIFTimersTest()
        {
            UpdateSIFTimers();
        }

        public string GetSIFTimeRemaining(string query)
        {
            DateTime end;
            UpdateSIFCachedData();

            if (String.Equals(query, "sifen"))
            {
                end = DateTime.Parse(sifenEvent.english_end);
            }
            else if (String.Equals(query, "sifjp"))
            {
                end = DateTime.Parse(sifjpEvent.end);
            }
            else
            {
                return "no query";
            }

            if (end < DateTime.Now)
            {
                return "finished";
            }
            else
            {
                return (string.Format("{0:dd\\:hh\\:mm\\:ss}", end - DateTime.Now));
            }
        }

        private void GetSIFTimeRemainingTest()
        {
            var a = GetSIFTimeRemaining("sifen");
            var b = GetSIFTimeRemaining("sifjp");
        }
        #endregion

        #region Obsolete Methods
        [Obsolete]
        private void Write(object content, string filePath)
        {
            string json = JsonConvert.SerializeObject(content, Formatting.Indented);
            System.IO.File.WriteAllText($"{filePath}", json);
        }
        [Obsolete]
        private void LoadJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath} is missing.");
            timerJson = JObject.Parse(File.ReadAllText(filePath));
        }
        [Obsolete]
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
        [Obsolete("Use GetStarlightTimeRemaining or GetSIFTimeRemaining")]
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
        [Obsolete("Use GetStarlightTimeRemaining or GetSIFTimeRemaining")]
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
        [Obsolete]
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
        #endregion

        #region Starlight Stage Classes
        private class SSTimer
        {
            [JsonProperty("gachas")]
            public JArray gachas { get; set; }
            [JsonProperty("events")]
            public JArray events { get; set; }
        }

        private class Gachas
        {
            [JsonProperty("id")]
            public string id { get; set; }
            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("start_date")]
            public double start { get; set; }
            [JsonProperty("end_date")]
            public double end { get; set; }
            [JsonProperty("type")]
            public string type { get; set; }
            [JsonProperty("subtype")]
            public string subtype { get; set; }
            [JsonProperty("rates")]
            public Rates rates { get; set; }
        }

        private class Rates
        {
            [JsonProperty("r")]
            public string r { get; set; }
            [JsonProperty("sr")]
            public string sr { get; set; }
            [JsonProperty("ssr")]
            public string ssr { get; set; }
        }

        private class Events
        {
            [JsonProperty("id")]
            public string id { get; set; }
            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("start_date")]
            public double start { get; set; }
            [JsonProperty("end_date")]
            public double end { get; set; }
        }
        #endregion

        #region School Idol Festival Classes
        private class SIFCachedData
        {
            [JsonProperty("current_event_en")]
            public SIFCachedEvent eventEn { get; set; }
            [JsonProperty("current_contests")]
            public JArray contests { get; set; }
            [JsonProperty("cards_info")]
            public JObject cardsInfo { get; set; }
            [JsonProperty("current_event_jp")]
            public SIFCachedEvent eventJp { get; set; }
        }

        private class SIFCachedEvent
        {
            [JsonProperty("image")]
            public string image { get; set; }
            [JsonProperty("japanese_name")]
            public string japanese_name { get; set; }
            [JsonProperty("slide_position")]
            public string slide_position { get; set; }
        }

        private class SIFEvent
        {
            [JsonProperty("japanese_name")]
            public string japanese_name { get; set; }
            [JsonProperty("romaji_name")]
            public string romaji_name { get; set; }
            [JsonProperty("english_name")]
            public string english_name { get; set; }
            [JsonProperty("translated_name")]
            public string translated_name { get; set; }
            [JsonProperty("image")]
            public string image { get; set; }
            [JsonProperty("english_image")]
            public string english_image { get; set; }
            [JsonProperty("beginning")]
            public string beginning { get; set; }
            [JsonProperty("end")]
            public string end { get; set; }
            [JsonProperty("english_beginning")]
            public string english_beginning { get; set; }
            [JsonProperty("english_end")]
            public string english_end { get; set; }
            [JsonProperty("japan_current")]
            public bool japan_current { get; set; }
            [JsonProperty("world_current")]
            public bool world_current { get; set; }
            [JsonProperty("english_status")]
            public string english_status { get; set; }
            [JsonProperty("japan_status")]
            public string japan_status { get; set; }
            [JsonProperty("japanese_t1_points")]
            public string japanese_t1_points { get; set; }
            [JsonProperty("japanese_t1_rank")]
            public string japanese_t1_rank { get; set; }
            [JsonProperty("japanese_t2_points")]
            public string japanese_t2_points { get; set; }
            [JsonProperty("japanese_t2_rank")]
            public string japanese_t2_rank { get; set; }
            [JsonProperty("english_t1_points")]
            public string english_t1_points { get; set; }
            [JsonProperty("english_t1_rank")]
            public string english_t1_rank { get; set; }
            [JsonProperty("english_t2_points")]
            public string english_t2_points { get; set; }
            [JsonProperty("english_t2_rank")]
            public string english_t2_rank { get; set; }
            [JsonProperty("note")]
            public string note { get; set; }
            [JsonProperty("website_url")]
            public string website_url { get; set; }
        }

        #endregion
    }
}
