using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;
using PlaytimeCounter.Enums;
using PlaytimeCounter.Features.Discord;
using PluginAPI.Core;
using Serialization;
using YamlDotNet;

namespace PlaytimeCounter.Features
{
    public class TrackingGroup
    {
        public static List<TrackingGroup> TrackingGroups = new List<TrackingGroup>();

        public string Name;
        public TrackingGroupConfig Config;
        public CountingType CountingType;

        public string groupDir;
        public string trackedUsersDir;

        public List<TrackedUser> trackedUsers;

        public List<UserGroup> groupsToLog;
        public List<string> idsToLog;

        public List<RoleTypeId> rolesToTrack;

        public SummaryTimerConfig summaryConfig;
        public DiscordConfig discordConfig;

        public bool _dntIgnored
        {
            get
            {
                if (Config == null)
                    return false;

                if (Server.PermissionsHandler.IsVerified)
                    return false;

                return Config.IgnoreDNT;
            }
        }

        public bool _discordWebhookEnabled
        {
            get
            {
                if (Config == null)
                    return false;

                if (Config.DiscordConfig == null)
                    return false;

                return Config.DiscordConfig.DiscordWebhookURL == "";
            }
        }

        public static void LoadTrackingGroups(string configDir)
        {
            if(Plugin.Instance.Config.TrackingGroups == null || Plugin.Instance.Config.TrackingGroups.Count < 1)
            {
                Log.Warning($"TrackingGroups does not contain any elements. Wrong configuration?");
                return;
            }

            try
            {
                foreach (string configTracker in Plugin.Instance.Config.TrackingGroups)
                {
                    string groupPath = Path.Combine(configDir, configTracker);
                    if (!Directory.Exists(groupPath))
                    {
                        Log.Info($"TrackingGroup {configTracker} does not exist! Creating one now...");
                        CreateGroup(configTracker, configDir);
                    }

                    Helpers.LogDebug("Checkpoint 1");

                    TrackingGroupConfig trackingGroupConfig = YamlParser.Deserializer.Deserialize<TrackingGroupConfig>(File.ReadAllText(Path.Combine(groupPath, "config.yml")));
                    TrackingGroup trackingGroup = new TrackingGroup();

                    Helpers.LogDebug("Checkpoint 2");

                    trackingGroup.Name = configTracker;
                    trackingGroup.Config = trackingGroupConfig;
                    trackingGroup.CountingType = trackingGroupConfig.CountingType;
                    trackingGroup.groupDir = groupPath;
                    trackingGroup.trackedUsersDir = Path.Combine(groupPath, "TrackedUsers");
                    trackingGroup.rolesToTrack = trackingGroupConfig.RolesToTrack;
                    trackingGroup.summaryConfig = trackingGroupConfig.SummaryTimerConfig;
                    trackingGroup.discordConfig = trackingGroupConfig.DiscordConfig;

                    Helpers.LogDebug("Checkpoint 3");

                    switch(trackingGroup.CountingType)
                    {
                        case CountingType.Group: 
                            trackingGroup.groupsToLog = new List<UserGroup>();
                            trackingGroupConfig.TrackingTargets.ForEach(x => trackingGroup.groupsToLog.Add(x.GetGroupFromString()));
                            break;
                        case CountingType.User: 
                            trackingGroup.idsToLog = trackingGroupConfig.TrackingTargets; 
                            break;
                    }

                    Helpers.LogDebug("Checkpoint 4");

                    trackingGroup.trackedUsers = new List<TrackedUser>();

                    foreach(string file in Directory.GetFiles(trackingGroup.trackedUsersDir))
                    {
                        string content = File.ReadAllText(file);
                        TrackedUser trackedUser = new TrackedUser();
                        try
                        {
                            trackedUser = YamlParser.Deserializer.Deserialize<TrackedUser>(content);
                        }
                        catch(Exception ex) 
                        {
                            Log.Error($"Failed reading TrackedUser data for {configTracker}! Exception: {ex.Message}");
                        }
                        trackingGroup.trackedUsers.Add(trackedUser);
                    }

                    Helpers.LogDebug("Checkpoint 5");

                    TrackingGroups.Add(trackingGroup);
                    Helpers.LogDebug("Checkpoint 6");
                    Log.Info($"Successfully loaded TrackingGroup {configTracker}!");
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Failed loading groups! Exception: {ex.Message}");
                return;
            }
            Plugin.Instance.GroupsRegistered = true;
        }

        public static void CreateGroup(string groupName, string configDir)
        {
            try
            {
                DirectoryInfo groupDir = Directory.CreateDirectory(Path.Combine(configDir, groupName));
                Directory.CreateDirectory(Path.Combine(groupDir.FullName, "TrackedUsers"));

                TrackingGroupConfig dummyConfig = new TrackingGroupConfig();
                File.WriteAllText(Path.Combine(groupDir.FullName, "config.yml"), YamlParser.Serializer.Serialize(dummyConfig));

                Log.Info($"Successfully created TrackingGroup {groupName}!");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed creating group! Exception: {ex.Message}");
                return;
            }
        }
    }

    public class TrackingGroupConfig
    {
        [Description("If set to true, PlaytimeCounter will not ignore players with DNT enabled when counting. **Using this setting on a verified server is blocked by default, as it may be a VSR violation unless you received proper clearance from tracked players. Read VSR 8.11.5 for more info.**")]
        public bool IgnoreDNT { get; set; } = false;

        [Description("If set to true, will only count playtime when the round is started.")]
        public bool CountOnlyWhenRoundStarted { get; set; } = false;

        [Description("Determines if individual users should be tracked instead of groups.")]
        public CountingType CountingType { get; set; } = CountingType.Group;

        [Description("List of Groups / UserIDs of people that are to be tracked. Whether you should put UserIDs in there instead of groups relies on the CountingType config.")]
        public List<string> TrackingTargets { get; set; } = new List<string>()
        {
            "owner"
        };

        [Description("Roles to track playtime of. If left empty, will only track 'global' and 'alive' playtime")]
        public List<RoleTypeId> RolesToTrack { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.Overwatch
        };

        [Description("Config section dedicated to Discord logging.")]
        public DiscordConfig DiscordConfig { get; set; } = new DiscordConfig();

        [Description("Config section dedicated to summary timers.")]
        public SummaryTimerConfig SummaryTimerConfig { get; set; } = new SummaryTimerConfig();
    }
}
