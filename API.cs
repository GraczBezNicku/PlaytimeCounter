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
        public static void SendWebhook(Player p, long secondsPlayed)
        {
            long seconds = secondsPlayed;
            long minutes = secondsPlayed / 60;
            long hours = minutes / 60;

            string message = Plugin.instance.Config.webhookMessage.Replace("{time}", DateTime.Now.ToString()).Replace("{player}", p.Nickname).Replace("{seconds}", seconds.ToString()).Replace("{minutes}", minutes.ToString()).Replace("{hours}", hours.ToString());

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
        }
    }
}
