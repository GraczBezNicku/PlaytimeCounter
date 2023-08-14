using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;
using PlaytimeCounter.Enums;
using PlaytimeCounter.Features.Discord;
using Serialization;

namespace PlaytimeCounter.Features
{
    public class TrackingGroup
    {
        public string Name;
        public TrackingGroupConfig Config;

        public string groupDir;
        public string trackedUsersDir;

        public List<TrackedUser> trackedUsers;

        public List<UserGroup> groupsToLog;
        public List<string> idsToLog;

        public List<RoleTypeId> rolesToTrack;

        public SummaryTimerConfig summaryConfig;
        public DiscordConfig discordConfig;

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
    }

    public class TrackingGroupConfig
    {
        [Description("If set to true, PlaytimeCounter will not ignore players with DNT enabled when counting. This will not work on verified servers. Modyfing this plugin's assembly to force this setting to work on verified servers may be a VSR violation.")]
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
