using PlaytimeCounter.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;
        public bool DebugMode { get; set; } = false;

        [Description("Lists all tracking groups. If a group with this name doesn't exist, it will automatically be created.")]
        public List<string> TrackingGroups { get; set; }

        [Description("Global Cooldown which will be applied on all outgoing webhooks. Recommended value is 5 to avoid rate limits.")]
        public int DiscordWebhookCooldown { get; set; } = 5;
    }
}
