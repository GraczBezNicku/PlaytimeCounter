using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounterNWAPI
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true;
        public bool DebugMode { get; set; } = true;

        [Description("This allows me to see how many people actually use this plugin. It will only inform me of the following: Server Name, Server IP & Port, Time at which your server started.")]
        public bool ServerTracking { get; set; } = true;

        [Description("List of groups to log playtime of.")]
        public List<string> groupsToLog { get; set; } = new List<string>();

        [Description("List of groups and their playtime requirement in seconds. If met, reqResult will change based on your config.")]
        public Dictionary<string, long> groupReqs { get; set; } = new Dictionary<string, long>()
        {
            {"owner",  12600}
        };

        [Description("List of groups and their playtime requirement (in overwatch mode) in seconds. If met, reqResultOv will change based on your config.")]
        public Dictionary<string, long> groupReqsOv { get; set; } = new Dictionary<string, long>()
        {
            {"owner", 12600 }
        };

        public string webhookURL { get; set; } = "";
        public string webhookAvatarURL { get; set; } = "https://cdn.discordapp.com/attachments/434037173281488899/940610688760545290/mrozonyhyperthink.jpg";

        public int discordWebhookCooldown { get; set; } = 10;

        public int summaryCheckCooldown { get; set; } = 300;
        [Description("First line when printing playtime summary")]
        public string webhookCountMessage { get; set; } = "Playtime summary: \n";
        [Description("Lines printed out for users which have playtime recorded. Accepts parameters: {steamID64}, {nickname}, {group}, {hours}, {minutes}, {seconds}, {hoursOv}, {minutesOv}, {secondsOv}, {reqResult}, {reqResultOv}")]
        public string webhookCountUserMessage { get; set; } = "**{nickname} ({steamID64}) [{group}]** has played for **{minutes}m ({minutesOv})** - Normal criteria met? {reqResult}, Overwatch criteria met? {reqResultOv} \n";

        public string webhookReqResultMet { get; set; } = ":white_check_mark:";
        public string webhookReqResultNotMet { get; set; } = ":x:";
        public string webhookReqResultUnknown { get; set; } = ":warning:";

        [Description("Message that will display on discord when a tracked player leaves the server. (can use {seconds} and {hours} too)")]
        public string webhookMessage { get; set; } = "{time} **{player}** left the server after playing for **{minutes}** minutes";

        [Description("Message that will display on discord when a tracked player leaves Overwatch mode. (can use {seconds} and {hours} too)")]
        public string webhookMessageOverwatch { get; set; } = "{time} **{player}** left Overwatch mode after playing for **{minutes}** minutes";
    }
}
