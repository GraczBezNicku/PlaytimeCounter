using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features.Discord
{
    public class DiscordConfig
    {
        [Description("Webhook URL where messages are going to be sent. If empty, messages are disabled.")]
        public string DiscordWebhookURL { get; set; } = "";

        public string DiscordWebhookUsername { get; set; } = "PlaytimeCounter";
        public string DiscordWebhookAvatarUrl { get; set; } = "https://cdn.discordapp.com/attachments/434037173281488899/940610688760545290/mrozonyhyperthink.jpg";

        [Description("Webhook message which will be sent when a player joins. (If counting only in round is enabled, then this will be sent when the round starts or when a player joins during a round) If empty, message will not be sent. Supports dynamic values: %TIME%, %NAME%, %USERID%, %GROUP%")]
        public string DiscordPlayerJoinedMessage { get; set; } = "**[%TIME%] Player %NAME% (%USERID%) [%GROUP%] has joined the server.**";

        [Description("Webhook message which will be sent when a player leaves. If empty, message will not be sent. Supports dynamic values: %TIME%, %HOURS%, %MINUTES%, %SECONDS%, %NAME%, %USERID%, %GROUP%")]
        public string DiscordPlayerLeftMessage { get; set; } = "**[%TIME%] Player %NAME% (%USERID%) [%GROUP%] has left the server after playing for %MINUTES%m (%SECONDS%s)**";

        [Description("Webhook message which will be sent when a player changes their role to one contained within the tracked role list. If empty, message will not be sent. Supports dynamic values: %TIME%, %NAME%, %USERID%, %GROUP%, %OLDROLE%, %NEWROLE%")]
        public string DiscordPlayerChangedRoleToMessage { get; set; } = "**[%TIME%] Player %NAME% (%USERID%) [%GROUP%] has changed their role from %OLDROLE% to %NEWROLE%**";

        [Description("Webhook message which will be sent when a player changes their role from one contained within the tracked role list. If empty, message will not be sent. Supports dynamic values: %TIME%, %NAME%, %USERID%, %GROUP%, %OLDROLE%, %NEWROLE%, %HOURS%, %MINUTES%, %SECONDS%")]
        public string DiscordPlayerChangedRoleFromMessage { get; set; } = "**[%TIME%] Player %NAME% (%USERID%) [%GROUP%] has changed their role from %OLDROLE% to %NEWROLE% after playing for %MINUTES%m (%SECONDS%s)**";

        [Description("First line when printing a summary. If empty, message will not be sent. Supports dynamic values: %TIME%")]
        public string DiscordSummaryFirstMessage { get; set; } = "**[%TIME%]** Staff Playtime Summary:";

        [Description("This line is used per user in a summary. If empty, message will not be sent. Supports dynamic values: %{ROLE}HOURS%, %{ROLE}MINUTES%, %{ROLE}SECONDS%, %NAME%, %USERID%, %GROUP%. In place of {ROLE} put the desired role, example: %OVERWATCHHOURS%. You can also use for example %GLOBALHOURS%, %ALIVEHOURS%")]
        public string DiscordSummaryPerUserMessage { get; set; } = "**%NAME% (%USERID%) [%GROUP%] has globally played for %MINUTES%m (%SECONDS%s)**";
    }
}
