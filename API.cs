using MEC;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounterNWAPI
{
    public static class API
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static Queue<string> messagesQueue = new Queue<string>();
        static string message = "";

        public enum UpdateType
        {
            Normal,
            Overwatch
        }

        public static void SendWebhook(Player p, long secondsPlayed, UpdateType updateType)
        {
            long seconds = secondsPlayed;
            long minutes = secondsPlayed / 60;
            long hours = minutes / 60;
            string messageToSend;

            if(updateType == UpdateType.Normal)
                messageToSend = Plugin.Instance.Config.webhookMessage.Replace("{time}", DateTime.Now.ToString()).Replace("{player}", p.Nickname).Replace("{seconds}", seconds.ToString()).Replace("{minutes}", minutes.ToString()).Replace("{hours}", hours.ToString());
            else
                messageToSend = Plugin.Instance.Config.webhookMessageOverwatch.Replace("{time}", DateTime.Now.ToString()).Replace("{player}", p.Nickname).Replace("{seconds}", seconds.ToString()).Replace("{minutes}", minutes.ToString()).Replace("{hours}", hours.ToString());
            QueueMessage(messageToSend);
        }

        public static void QueueMessage(string messageToAdd)
        {
            messagesQueue.Enqueue(messageToAdd);
        }

        public static async Task Send(StringContent data, string url)
        {
            HttpResponseMessage responseMessage = await HttpClient.PostAsync(url, data);
            string responseMessageString = await responseMessage.Content.ReadAsStringAsync();
            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Error($"[{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}] A non-successful status code was returned by Discord when trying to post to webhook. Response Message: {responseMessageString} .");
                return;
            }
            else
                message = "";
        }

        public static void UpdateFiles(Player p, long minutesPlayed, UpdateType updateType)
        {
            long totalMinutesPlayed = 0;
            long totalMinutesOverwatched = 0;

            if(updateType == UpdateType.Overwatch)
            {
                if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.UserId}.txt")))
                {
                    string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, $"{p.UserId}.txt"));
                    totalMinutesPlayed = Convert.ToInt64(lines[0]);
                    totalMinutesOverwatched = Convert.ToInt64(lines[1]) + minutesPlayed;
                    Log.Debug("File exists, retrieving timePlayed and timeOverwatched...");
                    Log.Debug($"totalMinutesPlayed = file number ({Convert.ToInt64(lines[0])})");
                    Log.Debug($"totalMinutesOverwatched = file number ({Convert.ToInt64(lines[1])}) + {minutesPlayed}");
                }
                else
                {
                    totalMinutesPlayed = 0;
                    totalMinutesOverwatched = minutesPlayed;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.UserId}.txt")))
                {
                    string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, $"{p.UserId}.txt"));
                    totalMinutesPlayed = Convert.ToInt64(lines[0]) + minutesPlayed;
                    totalMinutesOverwatched = Convert.ToInt64(lines[1]);
                    Log.Debug("File exists, retrieving timePlayed and timeOverwatched...");
                    Log.Debug($"totalMinutesPlayed = file number ({Convert.ToInt64(lines[0])}) + {minutesPlayed}");
                    Log.Debug($"totalMinutesOverwatched = file number ({Convert.ToInt64(lines[1])})");
                }
                else
                {
                    totalMinutesPlayed = minutesPlayed;
                    totalMinutesOverwatched = 0;
                }
            }

            if (File.Exists(Path.Combine(Plugin.GroupDir, $"{p.UserId}")))
                File.Delete(Path.Combine(Plugin.GroupDir, $"{p.UserId}"));
            Log.Debug("If the file existed, it sure does not now.");

            string pGroup = ServerStatic.PermissionsHandler._members.TryGetValue(p.UserId, out string groupName) ? groupName : null;

            using (StreamWriter sw = File.CreateText(Path.Combine(Plugin.GroupDir, $"{p.UserId}.txt")))
            {
                sw.WriteLine($"{totalMinutesPlayed}");
                sw.WriteLine($"{totalMinutesOverwatched}");
                sw.WriteLine($"{p.Nickname}");
                sw.WriteLine($"{pGroup}");
            }
            Log.Debug("Created new file.");
        }

        public static IEnumerator<float> SendAfterCooldown()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.discordWebhookCooldown);
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
                        avatar_url = Plugin.Instance.Config.webhookAvatarURL
                    };
                    Log.Debug("Sending webhook.");
                    StringContent content = new StringContent(Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize<object>(SuccessWebHook)), Encoding.UTF8, "application/json");
                    _ = Send(content, Plugin.Instance.Config.webhookURL);
                }
            }
        }

        public static IEnumerator<float> CheckForSummaryTime()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.summaryCheckCooldown);
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

            summary = Plugin.Instance.Config.webhookCountMessage;

            DirectoryInfo di = new DirectoryInfo(Plugin.GroupDir);

            string steamID64 = "tempID";
            string nickname = "tempNickname";
            string groupName = "tempGroupName";
            string metReqs;
            string metReqsOv;
            long seconds = 1;
            long minutes = 1;
            long hours = 1;
            long secondsOv = 1;
            long minutesOv = 1;
            long hoursOv = 1;

            foreach (FileInfo fileInfo in di.GetFiles())
            {
                if (fileInfo.Name == "SummaryTimeCheck.txt")
                    continue;

                steamID64 = fileInfo.Name.Remove(fileInfo.Name.Length - 4, 4);
                string[] lines = File.ReadAllLines(fileInfo.FullName);

                if(lines.Length != 4)
                {
                    Log.Warning($"Ignoring {fileInfo.Name}, since it isn't formatted correctly.");
                    continue;
                }

                nickname = lines[2];
                groupName = lines[3];
                minutes = long.Parse(lines[0]);
                seconds = minutes * 60;
                hours = minutes / 60;
                minutesOv = long.Parse(lines[1]);
                secondsOv = minutesOv * 60;
                hoursOv = minutesOv / 60;

                try
                {
                    if (Plugin.Instance.Config.groupReqs.ContainsKey(groupName))
                    {
                        if (seconds >= Plugin.Instance.Config.groupReqs[groupName])
                            metReqs = Plugin.Instance.Config.webhookReqResultMet;
                        else
                            metReqs = Plugin.Instance.Config.webhookReqResultNotMet;
                    }
                    else
                        metReqs = Plugin.Instance.Config.webhookReqResultUnknown;
                }
                catch (Exception ex)
                {
                    metReqs = Plugin.Instance.Config.webhookReqResultUnknown;
                    Log.Error($"Exception! Couldn't fetch timeReq for user's group. IF YOU HAVE GROUP REQS EMPTY, IGNORE THIS ERROR. Exception: {ex.Message}");
                }

                try
                {
                    if (Plugin.Instance.Config.groupReqsOv.ContainsKey(groupName))
                    {
                        if (secondsOv >= Plugin.Instance.Config.groupReqsOv[groupName])
                            metReqsOv = Plugin.Instance.Config.webhookReqResultMet;
                        else
                            metReqsOv = Plugin.Instance.Config.webhookReqResultNotMet;
                    }
                    else
                        metReqsOv = Plugin.Instance.Config.webhookReqResultUnknown;
                }
                catch (Exception ex)
                {
                    metReqsOv = Plugin.Instance.Config.webhookReqResultUnknown;
                    Log.Error($"Exception! Couldn't fetch timeReqOv for user's group. IF YOU HAVE GROUP REQS OV EMPTY, IGNORE THIS ERROR. Exception: {ex.Message}");
                }

                summary += Plugin.Instance.Config.webhookCountUserMessage.Replace("{steamID64}", steamID64).Replace("{seconds}", $"{seconds}").Replace("{minutes}", $"{minutes}").Replace("{hours}", $"{hours}").Replace("{nickname}", nickname).Replace("{group}", groupName).Replace("{reqResult}", metReqs).Replace("{minutesOv}", $"{minutesOv}").Replace("{secondsOv}", $"{secondsOv}").Replace("{hoursOv}", $"{hoursOv}").Replace("{reqResultOv}", metReqsOv);
                Log.Debug("Read time file.");
            }

            Log.Debug("Queueing summary.");
            QueueMessage(summary);
        }
    }
}

