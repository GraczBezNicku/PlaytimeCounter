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
        /*
        FEATURES LIST:
        Port previous features. (Without tracker, it's now useless)
        Possible config updater?
        Limit summary results to specified amount of entries.
        Seperate folders for multiple groups.
        New config options (count only when round is started etc.)
        SummaryTimer based on folders. ^
        SummaryTimer options: (static (specific IDs), dynamic (specific groups))
        Save users in .json instead of .txt (saves playtime on all roles)
        RoleSpecific role tracking (and global as an option)
        */

        [PluginConfig]
        public Config Config;

        public static Plugin Instance { get; private set; }
        private Harmony _harmony;

        public bool GroupsRegistered = false;

        [PluginEntryPoint("PlaytimeCounter", "3.0.0", "Lets you track playitme of specified groups or people.", "GBN")]
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
            //Reload all configs here.
        }

        [PluginUnload]
        public void PluginUnload()
        {
            _harmony.UnpatchAll();
            _harmony = null;

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
