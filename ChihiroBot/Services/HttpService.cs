using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Discord;
using Discord.Net;

namespace ChihiroBot
{
    public class HttpService : IService
    {
        private HttpClient _http;
        //private readonly string RemBotClientID = "a8w2l2eabjdpbb4hlp3n8grsbot8fzj";

        void IService.Install(DiscordClient client)
        {
            _http = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseCookies = false,
                PreAuthenticate = false //We do auth ourselves
            });
            _http.DefaultRequestHeaders.Add("accept", "*/*");
            _http.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            _http.DefaultRequestHeaders.Add("user-agent", client.Config.UserAgent);
        }

        public Task<HttpContent> Send(HttpMethod method, string path, string authToken = null)
            => Send<object>(method, path, null, authToken);
        public async Task<HttpContent> Send<T>(HttpMethod method, string path, T payload, string authToken = null)
            where T : class
        {
            HttpRequestMessage msg = new HttpRequestMessage(method, path);
            //if (path.Contains("api.twitch.tv"))
            //    msg.Headers.Add("Client-ID", RemBotClientID);
            if (authToken != null)
                msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            if (payload != null)
            {
                string json = JsonConvert.SerializeObject(payload);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _http.SendAsync(msg, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
                throw new HttpException(response.StatusCode);
            return response.Content;
        }
    }
}
