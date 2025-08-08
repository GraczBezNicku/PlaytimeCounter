using HarmonyLib;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Plugins;
using PlaytimeCounter.Features;
using System;
using System.IO;

namespace PlaytimeCounter
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "PlaytimeCounter";
        public override string Description => "Lets you track playitme of specified groups or people";
        public override string Author => "GBN";
        public override Version Version => new Version(3, 1, 0);
        public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;

        private Harmony _harmony;

        public bool GroupsRegistered = false;

        public override void Enable()
        {
            Instance = this;

            _harmony = new Harmony($"GBN-PLAYTIMECOUNTER-{DateTime.Now}");
            _harmony.PatchAll();

            LabApi.Events.Handlers.ServerEvents.WaitingForPlayers += RegisterAllGroups;
        }

        public override void Disable()
        {
            LabApi.Events.Handlers.ServerEvents.WaitingForPlayers -= RegisterAllGroups;

            _harmony.UnpatchAll();
            _harmony = null;

            TrackingGroup.TrackingGroups.ForEach(x => TrackingGroup.DestroyGroup(x.Name));

            Instance = null;
        }

        public void RegisterAllGroups()
        {
            if (Plugin.Instance.GroupsRegistered)
                return;

            TrackingGroup.LoadTrackingGroups(Path.Combine(PathManager.Configs.FullName, "PlaytimeCounter"));
        }
    }
}
