using Discord;
using Discord.Commands;
using Discord.Modules;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net;
using System.IO;
using ChihiroBot.Modules.Timer;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChihiroBot.Modules.Border
{
    internal class BorderModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private TimerModule tm = new TimerModule();
        private List<Keys> keys = new List<Keys>();

        private static string filePath2 = "./config/tweetinvi.json";
        public const string sifen_border = "sifen_trackbot";
        public const string sifjp_border = "sifjp_trackbot";
        public const string deresute_border = "deresute_border";
        private static string helpMessage = $"Usage: border <game>\nss - Starlight Stage\nsifen - School Idol Festival English\nsifjp - School Idol Festival Japanese";

        #region Obsolete Fields
        [Obsolete]
        private string ssCsv = "http://deresuteborder.web.fc2.com/csv/event_latest.csv";
        [Obsolete]
        private string sifenCsv = "https://docs.google.com/spreadsheets/d/1a2ihrwVgyZnjy3OjqKYsFyJLxECXBO5WrPkEK1WEivw/export?format=csv&id=1a2ihrwVgyZnjy3OjqKYsFyJLxECXBO5WrPkEK1WEivw&gid=2089803644";
        [Obsolete]
        private string sifjpCsv = "http://llborder.web.fc2.com/summary.csv";
        [Obsolete]
        private string result, csv, current, previous;
        [Obsolete]
        private string[] splitCsv, a, b;
        [Obsolete]
        private int[] d;
        [Obsolete]
        private object[] args;
        [Obsolete]
        private TimeSpan elapsed;
        #endregion

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            LoadKeys();
            Auth.SetUserCredentials(keys[0].consumerKey, keys[0].consumerSecret, keys[0].accessToken, keys[0].accessTokenSecret);
            manager.CreateCommands("border", group =>
            {
                group.CreateCommand("")
                       .Description("Returns the usage for Border module.")
                       .Do(e =>
                       {
                           return _client.Reply(e, helpMessage);
                       });
                group.CreateCommand("help")
                       .Description("Returns the usage for Border module.")
                       .Do(e =>
                       {
                           return _client.Reply(e, helpMessage);
                       });
                group.CreateCommand("ss")
                    .Description("Returns the current Starlight Stage tier borders.")
                    .Do(e =>
                    {
                        GetLastBorderTweet(e, deresute_border);
                    });
                group.CreateCommand("sifen")
                    .Description("Returns the current School Idol Festival EN tier borders.")
                    .Do(e =>
                    {
                        GetLastBorderTweet(e, sifen_border);
                    });
                group.CreateCommand("sifjp")
                    .Description("Returns the current School Idol Festival JP tier borders.")
                    .Do(e =>
                    {
                        GetLastBorderTweet(e, sifjp_border);
                    });
            });
            manager.CreateCommands("", group =>
            {
                group.CreateCommand("sifen")
                    .Description("Returns the current School Idol Festival EN tier borders.")
                    .Do(e =>
                    {
                        GetLastBorderTweet(e, sifen_border);
                    });
                group.CreateCommand("ss")
                    .Description("Returns the current Starlight Stage tier borders.")
                    .Do(e =>
                    {
                        GetLastBorderTweet(e, deresute_border);
                    });
                group.CreateCommand("sifjp")
                    .Description("Returns the current School Idol Festival JP tier borders.")
                    .Do(e =>
                    {
                        GetLastBorderTweet(e, sifjp_border);
                    });
            });
        }

        public async void GetLastBorderTweet(CommandEventArgs e, string account)
        {
            var accts = Search.SearchUsers(account);

            var lastTweets = accts.First().GetUserTimeline(10);
            var lastTweet = "";
            if (String.Equals(account, sifen_border))
            {
                foreach (var tweet in lastTweets)
                {
                    if (tweet.Text.Contains("T1:") && (tweet.Text.Contains("Time:") || tweet.Text.Contains("[FINAL]")))
                    {
                        lastTweet = tweet.Text;
                        break;
                    }
                }
                await e.Channel.SendMessage($"Remaining: {tm.GetSIFTimeRemaining("sifen")}\n{lastTweet}");
                return;
            }
            else if (String.Equals(account, sifjp_border))
            {
                foreach (var tweet in lastTweets)
                {
                    if (tweet.Text.Contains("T1:") && (tweet.Text.Contains("Time:") || tweet.Text.Contains("[FINAL]")))
                    {
                        lastTweet = tweet.Text;
                        break;
                    }
                }
                await e.Channel.SendMessage($"Remaining: {tm.GetSIFTimeRemaining("sifjp")}\n{lastTweet}");
                return;

            }
            else if (String.Equals(account, deresute_border))
            {
                foreach (var tweet in lastTweets)
                {
                    if (tweet.Text.Contains("2千位：") && tweet.Text.Contains("1万位："))
                    {
                        lastTweet = tweet.Text;
                        break;
                    }
                }
                await e.Channel.SendMessage($"Remaining: {tm.GetStarlightTimeRemaining("event")}\n{lastTweet.Split('#')[0]}");
                return;
            }

            await e.Channel.SendMessage($"Couldn't use {account} to find a border.");
            return;
        }

        private void LoadKeys()
        {
            using (StreamReader r = new StreamReader(filePath2))
            {
                string json = r.ReadToEnd();
                keys = JsonConvert.DeserializeObject<List<Keys>>(json);
            }
        }

        #region Obsolete Methods
        [Obsolete]
        private string GetCSV(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            return results;
        }

        [Obsolete]
        public async void GetBorderSS(CommandEventArgs e, string target)
        {
            await e.Channel.SendIsTyping();
            csv = GetCSV(ssCsv);
            splitCsv = csv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            current = splitCsv.GetValue(splitCsv.Count() - 2).ToString();
            previous = splitCsv.GetValue(splitCsv.Count() - 3).ToString();
            a = current.Split(',');
            b = previous.Split(',');

            d = new int[] { 0, Int32.Parse(a[1]) - Int32.Parse(b[1]), Int32.Parse(a[2]) - Int32.Parse(b[2]), Int32.Parse(a[3]) - Int32.Parse(b[3]), Int32.Parse(a[4]) - Int32.Parse(b[4]), Int32.Parse(a[5]) - Int32.Parse(b[5]) };
            elapsed = DateTime.Parse(a[0]) - DateTime.Parse(b[0]);

            args = new object[] { a[0], a[1], a[2], a[3], a[4], a[5], d[1], d[2], d[3], d[4], d[5], elapsed.TotalMinutes, tm.GetStarlightTimeRemaining("event") };

            result = String.Format("Remaining: {12}\nLast Updated: {0} JST (+{11} min)\nT1: {1} (+{6})\nT2: {2} (+{7})\nT3: {3} (+{8})\nT4: {4} (+{9})\nT5: {5} (+{10})", args);
            await e.Channel.SendMessage($"{result}");
        }

        [Obsolete("The .csv are no longer maintained, use GetLastBorderTweet instead")]
        private async Task GetBorderSIFEN(CommandEventArgs e, string target)
        {
            await e.Channel.SendIsTyping();
            csv = GetCSV(sifenCsv);
            splitCsv = csv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            int i = 0;
            foreach (string entry in splitCsv)
            {
                string[] splitEntry = entry.Split(',');
                if (String.IsNullOrEmpty(splitEntry[3]))
                {
                    break;
                }
                i++;
            }
            b = splitCsv[i - 2].Split(',');
            a = splitCsv[i - 1].Split(',');
            try
            {
                elapsed = DateTime.Parse(a[1]) - DateTime.Parse(b[1]);
                args = new object[] { a[1], a[4], a[5], a[6], a[7], a[9], a[10], a[11], a[12], elapsed.TotalMinutes, tm.GetTimer("sifen", "event") };
            }
            catch (Exception)
            {
                args = new object[] { a[1], a[4], a[5], a[6], a[7], a[9], a[10], a[11], a[12], "null", tm.GetTimer("sifen", "event") };
            }

            result = String.Format("Remaining: {10}\nLast Updated: {0} UTC (+{9} min)\nT1: {1} (+{5})\nT2: {2} (+{6})\nT3: {3} (+{7})\nT4: {4} (+{8})", args);
            await e.Channel.SendMessage($"{result}");
        }

        [Obsolete("The .csv are no longer maintained, use GetLastBorderTweet instead")]
        private async Task GetBorderSIFJP(CommandEventArgs e, string target)
        {
            await e.Channel.SendIsTyping();
            csv = GetCSV(sifjpCsv);
            splitCsv = csv.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            current = splitCsv.GetValue(splitCsv.Count() - 2).ToString();
            previous = splitCsv.GetValue(splitCsv.Count() - 3).ToString();
            a = current.Split(',');
            b = previous.Split(',');

            d = new int[] { 0, Int32.Parse(a[1]) - Int32.Parse(b[1]), Int32.Parse(a[2]) - Int32.Parse(b[2]), Int32.Parse(a[3]) - Int32.Parse(b[3]), Int32.Parse(a[4]) - Int32.Parse(b[4]), Int32.Parse(a[5]) - Int32.Parse(b[5]) };
            elapsed = DateTime.Parse(a[0]) - DateTime.Parse(b[0]);

            args = new object[] { a[0], a[1], a[2], a[3], a[4], a[5], d[1], d[2], d[3], d[4], d[5], elapsed.TotalMinutes, tm.GetTimer("sifjp", "event") };

            result = String.Format("Remaining: {12}\nLast Updated: {0} JST (+{11} min)\nT1: {1} (+{6})\nT2: {2} (+{7})\nT3: {3} (+{8})\nT4: {4} (+{9})", args);
            await e.Channel.SendMessage($"{result}");
        }
        #endregion

        internal class Keys
        {
            public string consumerKey = null;
            public string consumerSecret = null;
            public string accessToken = null;
            public string accessTokenSecret = null;
        }
    }
}
