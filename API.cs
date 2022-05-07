using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public static class API
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static void SendWebhook(Player p, long secondsPlayed)
        {
            long seconds = secondsPlayed;
            long minutes = secondsPlayed / 60;
            long hours = minutes / 60;

            string message = Plugin.instance.Config.webhookMessage.Replace("{time}", DateTime.Now.ToString()).Replace("{player}", p.Nickname).Replace("{seconds}", seconds.ToString()).Replace("{minutes}", minutes.ToString()).Replace("{hours}", hours.ToString());

            var SuccessWebHook = new
            {
                content = message,
            };

            StringContent content = new StringContent(Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize<object>(SuccessWebHook)), Encoding.UTF8, "application/json");
            _ = Send(content);
            /*
           new Thread(() =>
           {
               HttpClient httpClient = new HttpClient();

               var SuccessWebHook = new
               {
                   content = message,
               };

               var content = new StringContent(JsonConvert.SerializeObject(SuccessWebHook), Encoding.UTF8, "application/json");
               httpClient.PostAsync(Plugin.instance.Config.webhookURL, content).Wait();
           }).Start();
            */
        }

        public static async Task Send(StringContent data)
        {
            HttpResponseMessage responseMessage = await HttpClient.PostAsync(Plugin.instance.Config.webhookURL, data);
            string responseMessageString = await responseMessage.Content.ReadAsStringAsync();
            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Error($"[{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}] A non-successful status code was returned by Discord when trying to post to webhook. Response Message: {responseMessageString} .");
                return;
            }
        }
    }
}
