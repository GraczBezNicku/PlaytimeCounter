using Exiled.API.Features;
using MEC;
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

        public static Queue<string> messagesQueue = new Queue<string>();
        static string message = "";

        public static void SendWebhook(Player p, long secondsPlayed)
        {
            long seconds = secondsPlayed;
            long minutes = secondsPlayed / 60;
            long hours = minutes / 60;

            string messageToSend = Plugin.instance.Config.webhookMessage.Replace("{time}", DateTime.Now.ToString()).Replace("{player}", p.Nickname).Replace("{seconds}", seconds.ToString()).Replace("{minutes}", minutes.ToString()).Replace("{hours}", hours.ToString());
            QueueMessage(messageToSend);
        }

        public static void QueueMessage(string messageToAdd)
        {
            messagesQueue.Enqueue(messageToAdd);
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
            else
                message = "";
        }

        public static void UpdateFiles(Player p, long minutesPlayed)
        {
            long totalMinutesPlayed;

            if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}.txt")))
            {
                string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}.txt"));
                totalMinutesPlayed = Convert.ToInt64(lines[0]) + minutesPlayed;
                Log.Debug("File exists, retrieving timePlayed...");
                Log.Debug($"totalMinutesPlayed = file number ({Convert.ToInt64(lines[0])}) + minutes currently played ({minutesPlayed})");
            }
            else
                totalMinutesPlayed = minutesPlayed;

            if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}")))
                File.Delete(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}"));
            Log.Debug("If the file existed, it sure does not now.");

            using (StreamWriter sw = File.CreateText(Path.Combine(Plugin.GroupDir, $"{p.RawUserId}.txt")))
            {
                sw.WriteLine($"{totalMinutesPlayed}");
                sw.WriteLine($"{p.Nickname}");
                sw.WriteLine($"{p.GroupName}");
            }
            Log.Debug("Created new file.");
        }

        public static IEnumerator<float> SendAfterCooldown()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(Plugin.instance.Config.discordWebhookCooldown);
                Log.Debug("Time to send webhook.");
                while (message.Length < 2000)
                {
                    if (messagesQueue.Count == 0)
                        break;

                    string newMessage = message + messagesQueue.First();
                    if (newMessage.Length < 2000)
                    {
                        Log.Debug($"Adding new message {newMessage}");
                        message += messagesQueue.First();
                        messagesQueue.Dequeue();
                    }
                    else
                        break;
                }

                if (message != "")
                {
                    Log.Debug("Preparing webhook.");
                    var SuccessWebHook = new
                    {
                        username = "PlaytimeCounter",
                        content = message,
                        avatar_url = Plugin.instance.Config.webhookAvatarURL
                    };
                    Log.Debug("Sending webhook.");
                    StringContent content = new StringContent(Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize<object>(SuccessWebHook)), Encoding.UTF8, "application/json");
                    _ = Send(content);
                }
            }
        }

        public static IEnumerator<float> CheckForSummaryTime()
        {
            while(true)
            {
                yield return Timing.WaitForSeconds(Plugin.instance.Config.summaryCheckCooldown);
                Log.Debug("Time to make summary");

                if (!File.Exists(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt")))
                    continue;

                Log.Debug("Reading files for summary.");
                string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt"));
                long lastSummaryCheck = long.Parse(lines[0]);
                long summaryInterval = long.Parse(lines[1]);
                long nextSummaryCheck = lastSummaryCheck + summaryInterval;

                Log.Debug("Checking if should generate summary.");
                if (!(DateTimeOffset.Now.ToUnixTimeSeconds() >= nextSummaryCheck))
                    continue;

                PrepareSummary();

                Log.Debug("Deleting all files in GroupDir.");
                DirectoryInfo di = new DirectoryInfo(Plugin.GroupDir);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                Log.Debug("Creating new SummaryTimeCheckFile.txt");
                using (StreamWriter sw = File.CreateText(Path.Combine(Plugin.GroupDir, $"SummaryTimeCheck.txt")))
                {
                    sw.WriteLine($"{nextSummaryCheck}");
                    sw.WriteLine($"{summaryInterval}");
                }
            }
        }

        public static void PrepareSummary()
        {
            Log.Debug("Preparing summary.");
            string summary = "Temporary text";

            summary = Plugin.instance.Config.webhookCountMessage;

            DirectoryInfo di = new DirectoryInfo(Plugin.GroupDir);

            string steamID64 = "tempID";
            string nickname = "tempNickname";
            string groupName = "tempGroupName";
            string metReqs;
            long seconds = 1;
            long minutes = 1;
            long hours = 1;

            foreach(FileInfo fileInfo in di.GetFiles())
            {
                if (fileInfo.Name == "SummaryTimeCheck.txt")
                    continue;

                steamID64 = fileInfo.Name.Remove(fileInfo.Name.Length - 4, 4);
                string[] lines = File.ReadAllLines(fileInfo.FullName);
                nickname = lines[1];
                groupName = lines[2];
                minutes = long.Parse(lines[0]);
                seconds = minutes * 60;
                hours = minutes / 60;

                try
                {
                    if (Plugin.instance.Config.groupReqs.ContainsKey(groupName))
                    {
                        if (seconds >= Plugin.instance.Config.groupReqs[groupName])
                            metReqs = Plugin.instance.Config.webhookReqResultMet;
                        else
                            metReqs = Plugin.instance.Config.webhookReqResultNotMet;
                    }
                    else
                        metReqs = Plugin.instance.Config.webhookReqResultUnknown;
                }
                catch (Exception ex)
                {
                    metReqs = Plugin.instance.Config.webhookReqResultUnknown;
                    Log.Error($"Exception! Couldn't fetch time req for user's group. IF YOU HAVE GROUP REQS EMPTY, IGNORE THIS ERROR. Exception: {ex.Message}");
                }

                summary += Plugin.instance.Config.webhookCountUserMessage.Replace("{steamID64}", steamID64).Replace("{seconds}", $"{seconds}").Replace("{minutes}", $"{minutes}").Replace("{hours}", $"{hours}").Replace("{nickname}", nickname).Replace("{group}", groupName).Replace("{reqResult}", metReqs);
                Log.Debug("Read time file.");
            }

            Log.Debug("Queueing summary.");
            QueueMessage(summary);
        }
    }
}
