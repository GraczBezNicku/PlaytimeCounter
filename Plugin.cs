using HarmonyLib;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using PluginAPI.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PlaytimeCounterNWAPI
{
    public class Plugin
    {
        public static Plugin Instance;
        public static Assembly PlaytimeCounterAssembly = null; //This is a band-aid fix for the Debug option. This shouldn't cause conflicts with other plugins so if it does, please open an issue on Github. (Or DM me on Discord if I haven't responded in 3 days)
        public EventsHandler eventsHandler;

        public string TrackerSource = "http://pl1.mathost.eu:29092";
        public bool Tracked;

        public Dictionary<string, long> joinTime = new Dictionary<string, long>();
        public Dictionary<string, long> beginOverwatchTime = new Dictionary<string, long>();
        public static string GroupDir = Path.Combine(Paths.Configs, "PlaytimeCounter");

        public List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

        private Harmony _harmony;

        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("PlaytimeCounter", "2.3.1", "Automatically counts playtime of selected groups", "GBN#1862")]
        public void EntryPoint()
        {
            Instance = this;
            Tracked = false;
            eventsHandler = new EventsHandler();

            EventManager.RegisterAllEvents(this);

            PlaytimeCounterAssembly = Assembly.GetExecutingAssembly();

            if (!Directory.Exists(GroupDir))
            {
                Log.Warning("Directory where play times are stored does not exist. Don't worry though, we're creating one right now ;)");
                Directory.CreateDirectory(GroupDir);
            }

            _harmony = new Harmony($"GBN-PLAYTIMECOUNTER-{DateTime.Now}");
            _harmony.PatchAll();
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
        public void WaitingForPlayers()
        {
            if (Plugin.Instance.Config.ServerTracking && !Plugin.Instance.Tracked)
            {
                Timing.RunCoroutine(TrackServer());
            }
        }

        public IEnumerator<float> TrackServer()
        {
            using(UnityWebRequest webRequest = UnityWebRequest.Get(TrackerSource))
            {
                webRequest.SendWebRequest();

                yield return Timing.WaitForSeconds(20); //We'l just assume the server won't take longer than 10 seconds to load the page, which is a blank .html file.
                try
                {
                    string TrackerWebhook = webRequest.downloadHandler.text;

                    var TrackingWebhook = new
                    {
                        username = "PlaytimeCounter",
                        content = $"**Playtime Counter was launched!**\n**Server Name:** {ServerConsole._serverName}\n**Server IP:** {Server.ServerIpAddress}:{Server.Port}",
                        avatar_url = Plugin.Instance.Config.webhookAvatarURL
                    };
                    StringContent content = new StringContent(Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize<object>(TrackingWebhook)), Encoding.UTF8, "application/json");
                    _ = API.Send(content, TrackerWebhook);
                    Plugin.Instance.Tracked = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Server tracking failed! This does not stop the plugin from working, purely stops me from knowing who you are. (Please report. Exception: {ex.Message}, {ex.Source})");
                }
            }
        }
    }
}
