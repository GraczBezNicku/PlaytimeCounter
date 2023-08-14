﻿using System;
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
using PluginAPI.Events;
using Serialization;
using YamlDotNet;

namespace PlaytimeCounter.Features
{
    public class TrackingGroup
    {
        public static List<TrackingGroup> TrackingGroups = new List<TrackingGroup>();

        public TrackingGroup()
        {
            EventsHandler.PlayerJoinedEvent += OnPlayerJoined;
            EventsHandler.PlayerLeftEvent += OnPlayerLeft;
            EventsHandler.PlayerChangeRoleEvent += OnPlayerChangeRole;
            EventsHandler.RoundStartEvent += OnRoundStart;
            _roleTimeOffsets = new Dictionary<Player, long>();
            _globalJoinTimes = new Dictionary<Player, long>();
        }

        public string Name;
        public TrackingGroupConfig Config;
        public CountingType CountingType;

        public string groupDir;
        public string trackedUsersDir;

        public List<TrackedUser> trackedUsers;

        public List<UserGroup> groupsToLog;
        public bool TrackEveryone;
        public bool TrackNonGroups;
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

        private Dictionary<Player, long> _roleTimeOffsets;
        private Dictionary<Player, long> _globalJoinTimes;

        public void OnPlayerJoined(object sender, PlayerJoinedEvent ev)
        {
            if (Config.CountOnlyWhenRoundStarted && !Round.IsRoundStarted)
                return;

            if (!ShoudTrack(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname} would be tracked but they don't meet the requirements.");
                return;
            }

            if (_globalJoinTimes.ContainsKey(ev.Player))
                _globalJoinTimes.Remove(ev.Player);

            _globalJoinTimes.Add(ev.Player, DateTimeOffset.Now.ToUnixTimeSeconds());

            //Send webhook PlayerJoinedWebhook
        }

        public void OnPlayerLeft(object sender, PlayerLeftEvent ev)
        {
            if (!ShoudTrack(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname} would be tracked but they don't meet the requirements.");
                return;
            }

            if(!TrackedUser.TryGetTrackedUser(ev.Player, this, out TrackedUser trackedUser))
                trackedUser = TrackedUser.CreateTrackedUser(ev.Player, this);

            if(_globalJoinTimes.ContainsKey(ev.Player))
            {
                long finalTime = DateTimeOffset.Now.ToUnixTimeSeconds() - _globalJoinTimes[ev.Player];
                trackedUser.GlobalTime += finalTime;
                _globalJoinTimes.Remove(ev.Player);
                Helpers.LogDebug($"{ev.Player.Nickname} has left the game and got additional {finalTime} seconds onto GlobalTime.");
                TrackedUser.SaveTrackedUser(trackedUser, this);
                //Send webhook PlayerLeftWebhook
            }
        }

        public void OnPlayerChangeRole(object sender, PlayerChangeRoleEvent ev)
        {
            if (Config.CountOnlyWhenRoundStarted && !Round.IsRoundStarted)
                return;

            if (!ShoudTrack(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname} would be tracked but they don't meet the requirements.");
                return;
            }

            if (!TrackedUser.TryGetTrackedUser(ev.Player, this, out TrackedUser trackedUser))
                trackedUser = TrackedUser.CreateTrackedUser(ev.Player, this);

            if(rolesToTrack.Contains(ev.NewRole))
            {
                //Send webhook PlayerChangedRoleToWebhook
            }

            if(rolesToTrack.Contains(ev.OldRole.RoleTypeId))
            {
                long finalTime;

                if(_roleTimeOffsets.ContainsKey(ev.Player))
                {
                    Helpers.LogDebug($"{ev.Player.Nickname}'s active time offset is {_roleTimeOffsets[ev.Player]}");
                    finalTime = Convert.ToInt64(ev.OldRole.ActiveTime) - _roleTimeOffsets[ev.Player];
                }
                else
                    finalTime = Convert.ToInt64(ev.OldRole.ActiveTime);

                if (finalTime < 1)
                    finalTime = 0;

                _roleTimeOffsets.Remove(ev.Player);

                trackedUser.TimeTable[ev.OldRole.RoleTypeId] += finalTime;
                Helpers.LogDebug($"{ev.Player.Nickname} changed their role from {ev.OldRole.RoleTypeId} to {ev.NewRole} and increased their timetable for {ev.OldRole.RoleTypeId} by {finalTime}");
                TrackedUser.SaveTrackedUser(trackedUser, this);
                //Send PlayerChangedRoleFromWebhook
            }
        }

        public void OnRoundStart(object sender, RoundStartEvent ev)
        {
            if (!Config.CountOnlyWhenRoundStarted)
                return;

            _roleTimeOffsets.Clear();
            _globalJoinTimes.Clear();
            foreach(Player p in Player.GetPlayers())
            {
                if (!ShoudTrack(p))
                {
                    Helpers.LogDebug($"{p.Nickname} would be tracked but they don't meet the requirements.");
                    continue;
                }

                _roleTimeOffsets.Add(p, Convert.ToInt64(p.RoleBase.ActiveTime));
                _globalJoinTimes.Add(p, DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }

        public bool ShoudTrack(Player p)
        {
            if (p.DoNotTrack && !_dntIgnored)
                return false;

            if(CountingType == CountingType.User)
            {
                if (idsToLog.Contains(p.UserId))
                    return true;
                else
                    return false;
            }

            if (p.ReferenceHub.serverRoles.Group == null)
            {
                if (!TrackNonGroups && !TrackEveryone)
                    return false;
            }
            else
            {
                if (TrackEveryone)
                    return true;

                if (groupsToLog.Contains(p.ReferenceHub.serverRoles.Group))
                    return true;

                return false;
            }

            return true;
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
                            if (trackingGroupConfig.TrackingTargets.Contains("everyone"))
                            { 
                                trackingGroup.TrackEveryone = true;
                                break;
                            }
                            trackingGroup.groupsToLog = new List<UserGroup>();
                            trackingGroupConfig.TrackingTargets.ForEach(x => 
                            {
                                if (x == "default")
                                    trackingGroup.TrackNonGroups = true;
                                else
                                    trackingGroup.groupsToLog.Add(x.GetGroupFromString());
                            });
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

        public static void DestroyGroup(string groupName, string configDir) 
        { 
            if(!TrackingGroup.TrackingGroups.Any(x => x.Name == groupName))
            {
                Log.Error($"There is no group named {groupName}!");
                return;
            }

            TrackingGroup group = TrackingGroup.TrackingGroups.First(x => x.Name == groupName);

            EventsHandler.PlayerJoinedEvent -= group.OnPlayerJoined;
            EventsHandler.PlayerLeftEvent -= group.OnPlayerLeft;
            EventsHandler.PlayerChangeRoleEvent -= group.OnPlayerChangeRole;
            EventsHandler.RoundStartEvent -= group.OnRoundStart;

            TrackingGroups.Remove(group);
            group.trackedUsers.ForEach(x => TrackedUser.SaveTrackedUser(x, group));
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

        [Description("List of Groups / UserIDs of people that are to be tracked. Whether you should put UserIDs in there instead of groups relies on the CountingType config. 'default' will track people without groups and 'everyone' will track everyone.")]
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
