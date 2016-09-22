using Discord;
using Discord.Commands;
using Discord.Modules;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ChihiroBot.Modules.Finance
{
    internal class FinanceModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private string baseQuery = "https://query.yahooapis.com/v1/public/yql?q=";
        private string queryStart = "select%20*%20from%20yahoo.finance.xchange%20where%20pair%20in%20(%22";
        private string queryEnd = "%22)&format=json&diagnostics=true&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=";
        private dynamic queryResult;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            manager.CreateCommands("", group =>
            {
                group.CreateCommand("convert")
                    .Parameter("Text", ParameterType.Unparsed)
                    .Description("Converts currency A to currency B")
                    .Do(e =>
                    {
                        return GetExchangeRate(e);
                    });
            });
        }

        private async Task GetExchangeRate(CommandEventArgs e)
        {
            string[] args = e.Args[0].Split(' ');
            string query = baseQuery + queryStart + args[0] + args[2] + queryEnd;

            queryResult = GetJson(query);
            dynamic stuff = JObject.Parse(queryResult);
            string name = stuff.query.results.rate.Name;
            string rate = stuff.query.results.rate.Rate;

            await _client.Reply(e, $"Current {name} rate is {rate}.");
        }

        private object GetJson(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string results = sr.ReadToEnd();
            sr.Close();

            return results;
        }
    }
}
