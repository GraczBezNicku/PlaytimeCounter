using HarmonyLib;
using PlaytimeCounter.Features;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class Plugin
    {
        [PluginConfig]
        public Config Config;

        public static Plugin Instance { get; private set; }
        private Harmony _harmony;

        public bool GroupsRegistered = false;

        [PluginEntryPoint("PlaytimeCounter", "3.0.0-pre2", "Lets you track playitme of specified groups or people.", "GBN")]
        public void PluginLoad()
        {
            Instance = this;
            EventManager.RegisterAllEvents(this);

            _harmony = new Harmony($"GBN-PLAYTIMECOUNTER-{DateTime.Now}");
            _harmony.PatchAll();
        }

        [PluginReload]
        public void PluginReload()
        {
            //Reload all configs here. To be implemented.
        }

        [PluginUnload]
        public void PluginUnload()
        {
            _harmony.UnpatchAll();
            _harmony = null;

            TrackingGroup.TrackingGroups.ForEach(x => TrackingGroup.DestroyGroup(x.Name, PluginHandler.Get(this).PluginDirectoryPath));

            EventManager.UnregisterAllEvents(this);
            Instance = null;
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
        public void RegisterAllGroups(WaitingForPlayersEvent ev)
        {
            if (Plugin.Instance.GroupsRegistered)
                return;

            TrackingGroup.LoadTrackingGroups(PluginHandler.Get(Plugin.Instance).PluginDirectoryPath);
        }
    }
}
