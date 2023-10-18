using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using MEC;
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
            EventsHandler.RoundEndEvent += OnRoundEnd;
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

        public bool _dntIgnored
        {
            get
            {
                if (Config == null)
                    return false;

                /*
                if (Server.PermissionsHandler.IsVerified)
                    return false;
                */

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

                return Config.DiscordConfig.DiscordWebhookURL != "";
            }
        }

        private Dictionary<Player, long> _roleTimeOffsets;
        private Dictionary<Player, long> _globalJoinTimes;

        public void OnPlayerJoined(object sender, PlayerJoinedEvent ev)
        {
            if (Config.CountOnlyWhenRoundStarted && !Round.IsRoundStarted)
                return;

            if (!ShouldTrack(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname} would be tracked but they don't meet the requirements.");
                return;
            }

            if (_globalJoinTimes.ContainsKey(ev.Player))
                _globalJoinTimes.Remove(ev.Player);

            _globalJoinTimes.Add(ev.Player, DateTimeOffset.Now.ToUnixTimeSeconds());

            LogInternal($"{ev.Player.Nickname} has joined the server and is now being tracked");
            Helpers.LogDebug($"Webhooks enabled: {_discordWebhookEnabled}, Message: {Config.DiscordConfig.DiscordPlayerJoinedMessage}");

            if(_discordWebhookEnabled && Config.DiscordConfig.DiscordPlayerJoinedMessage != "")
            {
                PlayerJoinedWebhook webhook = new PlayerJoinedWebhook(Config.DiscordConfig.DiscordPlayerJoinedMessage, Config.DiscordConfig.DiscordWebhookURL, this, DateTime.Now, ev.Player.Nickname, ev.Player.UserId, ev.Player.GetGroupName());
                DiscordWebhookHandler.WebhookQueue.Enqueue(webhook);
            }
        }

        public void OnPlayerLeft(object sender, PlayerLeftEvent ev)
        {
            if (!ShouldTrack(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname} would be tracked but they don't meet the requirements.");
                return;
            }

            if(!TrackedUser.TryGetTrackedUser(ev.Player, this, out TrackedUser trackedUser))
                trackedUser = TrackedUser.CreateTrackedUser(ev.Player, this);

            if(_globalJoinTimes.ContainsKey(ev.Player))
            {
                LogInternal($"{ev.Player.Nickname} has left the server and their time information will be updated");
                long finalTime = DateTimeOffset.Now.ToUnixTimeSeconds() - _globalJoinTimes[ev.Player];
                trackedUser.GlobalTime += finalTime;
                _globalJoinTimes.Remove(ev.Player);
                Helpers.LogDebug($"{ev.Player.Nickname} has left the game and got additional {finalTime} seconds onto GlobalTime.");
                TrackedUser.SaveTrackedUser(trackedUser, this);
                
                if(_discordWebhookEnabled && Config.DiscordConfig.DiscordPlayerLeftMessage != "")
                {
                    PlayerLeftWebhook webhook = new PlayerLeftWebhook(Config.DiscordConfig.DiscordPlayerLeftMessage, 
                        Config.DiscordConfig.DiscordWebhookURL, 
                        this, 
                        DateTime.Now, 
                        finalTime, 
                        ev.Player.Nickname, 
                        ev.Player.UserId, 
                        ev.Player.GetGroupName());
                    DiscordWebhookHandler.WebhookQueue.Enqueue(webhook);
                }
            }
        }

        public void OnPlayerChangeRole(object sender, PlayerChangeRoleEvent ev)
        {
            if (Config.CountOnlyWhenRoundStarted && !Round.IsRoundStarted)
                return;

            if (!ShouldTrack(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname} would be tracked but they don't meet the requirements.");
                return;
            }

            if (!TrackedUser.TryGetTrackedUser(ev.Player, this, out TrackedUser trackedUser))
                trackedUser = TrackedUser.CreateTrackedUser(ev.Player, this);

            if(rolesToTrack.Contains(ev.NewRole))
            {
                LogInternal($"{ev.Player.Nickname} has changed their role from {ev.OldRole.RoleTypeId} to {ev.NewRole}");
                if(_discordWebhookEnabled && Config.DiscordConfig.DiscordPlayerChangedRoleToMessage != "")
                {
                    PlayerChangedRoleToWebhook webhook = new PlayerChangedRoleToWebhook(Config.DiscordConfig.DiscordPlayerChangedRoleToMessage,
                        Config.DiscordConfig.DiscordWebhookURL,
                        this,
                        DateTime.Now,
                        ev.Player.Nickname,
                        ev.Player.UserId,
                        ev.Player.GetGroupName(),
                        ev.OldRole.RoleTypeId,
                        ev.NewRole);
                    DiscordWebhookHandler.WebhookQueue.Enqueue(webhook);
                }
            }

            long finalTime;

            if (_roleTimeOffsets.ContainsKey(ev.Player))
            {
                Helpers.LogDebug($"{ev.Player.Nickname}'s active time offset is {_roleTimeOffsets[ev.Player]}");
                finalTime = Convert.ToInt64(ev.OldRole.ActiveTime) - _roleTimeOffsets[ev.Player];
            }
            else
                finalTime = Convert.ToInt64(ev.OldRole.ActiveTime);

            _roleTimeOffsets.Remove(ev.Player);

            if (finalTime < 1)
                finalTime = 0;

            if (ev.OldRole.RoleTypeId.IsAlive())
            {
                trackedUser.AliveTime += finalTime;
            }

            if (rolesToTrack.Contains(ev.OldRole.RoleTypeId))
            {
                trackedUser.TimeTable[ev.OldRole.RoleTypeId] += finalTime;
                LogInternal($"{ev.Player.Nickname} changed their role from {ev.OldRole.RoleTypeId} to {ev.NewRole} and increased their timetable for {ev.OldRole.RoleTypeId} by {finalTime}");
                Helpers.LogDebug($"{ev.Player.Nickname} changed their role from {ev.OldRole.RoleTypeId} to {ev.NewRole} and increased their timetable for {ev.OldRole.RoleTypeId} by {finalTime}");
                TrackedUser.SaveTrackedUser(trackedUser, this);
                
                if(_discordWebhookEnabled && Config.DiscordConfig.DiscordPlayerChangedRoleFromMessage != "")
                {
                    PlayerChangedRoleFromWebhook webhook = new PlayerChangedRoleFromWebhook(Config.DiscordConfig.DiscordPlayerChangedRoleFromMessage,
                        Config.DiscordConfig.DiscordWebhookURL,
                        this,
                        DateTime.Now,
                        ev.Player.Nickname,
                        ev.Player.UserId,
                        ev.Player.GetGroupName(),
                        finalTime,
                        ev.OldRole.RoleTypeId,
                        ev.NewRole);
                    DiscordWebhookHandler.WebhookQueue.Enqueue(webhook);
                }
            }
        }

        public void OnRoundStart(object sender, RoundStartEvent ev)
        {
            LogInternal($"ROUND HAS STARTED AT {DateTime.Now}");

            if (!Config.CountOnlyWhenRoundStarted)
                return;

            _roleTimeOffsets.Clear();
            _globalJoinTimes.Clear();
            foreach(Player p in Player.GetPlayers())
            {
                if (!ShouldTrack(p))
                {
                    Helpers.LogDebug($"{p.Nickname} would be tracked but they don't meet the requirements.");
                    continue;
                }

                _roleTimeOffsets.Add(p, Convert.ToInt64(p.RoleBase.ActiveTime));
                _globalJoinTimes.Add(p, DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }

        public void OnRoundEnd(object sender, RoundEndEvent ev)
        {
            LogInternal($"ROUND HAS ENDED AT {DateTime.Now}");
            Helpers.LogDebug($"Round has ended and has lasted {Round.Duration.TotalSeconds} (Rounded: {Convert.ToInt64(Round.Duration.TotalSeconds)})");

            Config.SummaryTimerConfig.RoundTimeBetweenSummaries += Convert.ToInt64(Round.Duration.TotalSeconds);

            if(Config.ReForceclassOnRoundEnd)
            {
                Timing.CallDelayed(1f, () =>
                {
                    foreach(Player p in Player.GetPlayers())
                    {
                        if (!ShouldTrack(p))
                            continue;

                        RoleTypeId oldRole = p.ReferenceHub.roleManager.CurrentRole.RoleTypeId;
                        p.ReferenceHub.roleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.RemoteAdmin, (RoleSpawnFlags)3);
                        Timing.CallDelayed(0.5f, () => p.ReferenceHub.roleManager.ServerSetRole(oldRole, RoleChangeReason.RemoteAdmin, (RoleSpawnFlags)3));
                    }
                });
            }

            SaveConfig();
        }

        public void SaveConfig()
        {
            LogInternal($"SaveConfig was called");
            try
            {
                TrackingGroupConfig currentConfigFile = YamlParser.Deserializer.Deserialize<TrackingGroupConfig>(File.ReadAllText(Path.Combine(groupDir, "config.yml")));

                currentConfigFile.SummaryTimerConfig = Config.SummaryTimerConfig;

                File.WriteAllText(Path.Combine(groupDir, "config.yml"), YamlParser.Serializer.Serialize(currentConfigFile));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed saving a TrackingGroup's config! Exception: {ex.Message}");
                return;
            }
        }

        public bool ShouldTrack(Player p)
        {
            if (p.UserId == "" || p.Nickname == "(null)")
                return false;

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

        public void LogInternal(string content)
        {
            if (!Config.InternalLogging)
                return;

            try
            {
                File.AppendAllText(Path.Combine(groupDir, "log.txt"), $"\n[{DateTime.Now}] [{Name.ToUpper()}] {content}");
            }
            catch(Exception ex)
            {
                Log.Error($"Failed internal logging for group {Name}! Exception: {ex.Message}");
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

        public static void DestroyGroup(string groupName) 
        { 
            if(!TrackingGroup.TrackingGroups.Any(x => x.Name == groupName))
            {
                Log.Error($"There is no group named {groupName}!");
                return;
            }

            TrackingGroup group = TrackingGroup.TrackingGroups.First(x => x.Name == groupName);

            group.LogInternal($"Group is about to be destroyed");

            EventsHandler.PlayerJoinedEvent -= group.OnPlayerJoined;
            EventsHandler.PlayerLeftEvent -= group.OnPlayerLeft;
            EventsHandler.PlayerChangeRoleEvent -= group.OnPlayerChangeRole;
            EventsHandler.RoundStartEvent -= group.OnRoundStart;
            EventsHandler.RoundEndEvent -= group.OnRoundEnd;

            group.SaveConfig();

            TrackingGroups.Remove(group);
            group.trackedUsers.ForEach(x => TrackedUser.SaveTrackedUser(x, group));
            group.trackedUsers.Clear();
        }
    }

    public class TrackingGroupConfig
    {
        [Description("If set to true, PlaytimeCounter will not ignore players with DNT enabled when counting. **Using this setting on a verified server is blocked by default, as it may be a VSR violation unless you received proper clearance from tracked players. Read VSR 8.11.5 for more info.**")]
        public bool IgnoreDNT { get; set; } = false;

        [Description("If set to true, enables internal logging, so that you can diagnose issues with the plugin. Logs will be kept in a single file called 'log.txt' in the groups directory")]
        public bool InternalLogging { get; set; } = true;

        [Description("If set to true, will reforceclass all users matching tracking settings to their original role, to artificially trigger the ChangeRole event")]
        public bool ReForceclassOnRoundEnd { get; set; } = true;

        [Description("If set to true, will only count playtime when the round is started.")]
        public bool CountOnlyWhenRoundStarted { get; set; } = false;

        [Description("If set to true, will allow tracked users from this group to see their playtime.")]
        public bool AppearInSelfCheck { get; set; } = true;

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
