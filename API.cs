using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static void UpdateFiles(Player p, long minutesPlayed)
        {
            long totalMinutesPlayed;

            if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}.txt")))
            {
                string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}.txt"));
                totalMinutesPlayed = Convert.ToInt64(lines[0]) + minutesPlayed;
                Log.Info("File exists, retrieving timePlayed...");
                Log.Info($"totalMinutesPlayed = file number ({Convert.ToInt64(lines[0])}) + minutes currently played ({minutesPlayed})");
            }
            else
                totalMinutesPlayed = minutesPlayed;

            if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}")))
                File.Delete(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}"));
            Log.Info("If the file existed, it sure does not now.");

            using (StreamWriter sw = File.CreateText(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}.txt")))
            {
                sw.WriteLine($"{totalMinutesPlayed}");
            }
            Log.Info("Created new file.");
        }
    }
}
