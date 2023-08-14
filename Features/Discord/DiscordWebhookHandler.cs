using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MEC;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace PlaytimeCounter.Features.Discord
{
    public static class DiscordWebhookHandler
    {
        //Create Webhook sending handler with queues.

        private static readonly HttpClient HttpClient = new HttpClient();
        public static Queue<DiscordWebhook> WebhookQueue = new Queue<DiscordWebhook>();
        public static Queue<DiscordWebhookBundle> WebhookBundleQueue = new Queue<DiscordWebhookBundle>();
        public static Queue<KeyValuePair<TrackingGroup, string>> MessagesQueue = new Queue<KeyValuePair<TrackingGroup, string>>();

        public static CoroutineHandle msgHandle, queueHandle;

        static string message, dedicatedMessage;

        public static async Task Send(StringContent data, string url)
        {
            HttpResponseMessage responseMessage = await HttpClient.PostAsync(url, data);
            string responseMessageString = await responseMessage.Content.ReadAsStringAsync();
            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Error($"[{(int)responseMessage.StatusCode} - {responseMessage.StatusCode}] A non-successful status code was returned by Discord when trying to post to webhook. Response Message: {responseMessageString} .");
                return;
            }
        }

        public static IEnumerator<float> MessageQueueCoroutine()
        {
            Helpers.LogDebug($"Messages coroutine started.");
            while (true)
            {
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.DiscordWebhookCooldown);
                Helpers.LogDebug($"MessagesQueue count is {MessagesQueue.Count()}");
                if(MessagesQueue.Count > 0)
                {
                    Helpers.LogDebug($"Preparing webhook.");
                    KeyValuePair<TrackingGroup, string> webhook = MessagesQueue.Dequeue();
                    var SuccessfullWebhook = new
                    {
                        username = webhook.Key.discordConfig.DiscordWebhookUsername,
                        content = webhook.Value,
                        avatar_url = webhook.Key.discordConfig.DiscordWebhookAvatarUrl
                    };
                    StringContent content = new StringContent(Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize<object>(SuccessfullWebhook)), Encoding.UTF8, "application/json");
                    _ = Send(content, webhook.Key.discordConfig.DiscordWebhookURL);
                }
            }
        }

        public static IEnumerator<float> WebhookQueueCoroutine()
        {
            Helpers.LogDebug($"Webhook coroutine started.");
            while(true)
            {
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.DiscordWebhookCooldown);
                Helpers.LogDebug($"Webhook coroutine PING!");
                if (WebhookBundleQueue.Count > 0)
                {
                    string msg = "";
                    DiscordWebhookBundle bundle = WebhookBundleQueue.Dequeue();
                    List<DiscordWebhook> webhooks = bundle.DiscordWebhooks.ToList();
                    TrackingGroup lastKnownGroup = null;

                    foreach (DiscordWebhook webhook in webhooks)
                    {
                        lastKnownGroup = webhook.RequestingGroup;

                        if(msg.Length + webhook.FormattedMessage().Length + 2 > 2000)
                        {
                            MessagesQueue.Enqueue(new KeyValuePair<TrackingGroup, string>(webhook.RequestingGroup, msg));
                            msg = "";
                        }

                        msg += $"{webhook.FormattedMessage()}\n";
                    }

                    if (msg.Length > 0)
                        MessagesQueue.Enqueue(new KeyValuePair<TrackingGroup, string>(lastKnownGroup, msg));
                }
                else if(WebhookQueue.Count > 0)
                {
                    Helpers.LogDebug($"WebhookQueue count is higher than 0");
                    string msg = "";
                    TrackingGroup lastKnownGroup = null;

                    while(WebhookQueue.Count > 0)
                    {
                        DiscordWebhook webhook = WebhookQueue.Dequeue();
                        Helpers.LogDebug($"Handling webhook requested by {webhook.RequestingGroup}");
                        if(webhook.FormattedMessage().Length > 2000)
                        {
                            Log.Error($"A single webhook cannot have text longer than 2000 characters!");
                            continue;
                        }

                        lastKnownGroup = webhook.RequestingGroup;

                        if(msg.Length + webhook.FormattedMessage().Length + 2 > 2000)
                        {
                            MessagesQueue.Enqueue(new KeyValuePair<TrackingGroup, string>(webhook.RequestingGroup, msg));
                            msg = "";
                        }

                        msg += $"{webhook.FormattedMessage()}\n";
                    }

                    if(msg.Length > 0)
                        MessagesQueue.Enqueue(new KeyValuePair<TrackingGroup, string>(lastKnownGroup, msg));
                }
            }
        }
    }
}
