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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounterNWAPI
{
    public class Plugin
    {
        public static Plugin Instance;
        public static Assembly PlaytimeCounterAssembly = null; //This is a band-aid fix for the Debug option. This shouldn't cause conflicts with other plugins so if it does, please open an issue on Github. (Or DM me on Discord if I haven't responded in 3 days)
        public EventsHandler eventsHandler;

        public Dictionary<string, long> joinTime = new Dictionary<string, long>();
        public static string GroupDir = Path.Combine(Paths.Configs, "PlaytimeCounter");

        public List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

        private Harmony _harmony;

        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("PlaytimeCounter", "2.2.0", "Automatically counts playtime of selected groups", "GBN#1862")]
        public void EntryPoint()
        {
            Instance = this;
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
    }
}
