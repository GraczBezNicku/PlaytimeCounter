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
        [Description("Webhook where 'left' messages are going to be sent. If empty, messages are disabled.")]
        public string DiscordWebhookURL { get; set; } = "";

        [Description("Webhook message which will be sent when a player joins. If empty, message will not be sent. Supports dynamic values: %TIME%, %NAME%, %USERID%, %GROUP%")]
        public string DiscordPlayerJoinedMessage { get; set; } = "";

        [Description("Webhook message which will be sent when a player leaves. If empty, message will not be sent. Supports dynamic values: %TIME%, %HOURS%, %MINUTES%, %SECONDS%, %NAME%, %USERID%, %GROUP%")]
        public string DiscordPlayerLeftMessage { get; set; } = "";

        [Description("Webhook message which will be sent when a player changes their role to one contained within the tracked role list. Supports dynamic values: %TIME%, %NAME%, %USERID%, %GROUP%, %OLDROLE%, %NEWROLE%")]
        public string DiscordPlayerChangedRoleTo { get; set; } = "";

        [Description("Webhook message which will be sent when a player changes their role from one contained within the tracked role list. Supports dynamic values: %TIME%, %NAME%, %USERID%, %GROUP%, %OLDROLE%, %NEWROLE%")]
        public string DiscordPlayerChangedRoleFrom { get; set; } = "";

        [Description("First line when printing a summary. Supports dynamic values: %TIME%")]
        public string DiscordSummaryFirstMessage { get; set; } = "";

        [Description("This line is used per user in a summary. Supports dynamic values: %{ROLE}HOURS%, %{ROLE}MINUTES%, %{ROLE}SECONDS%, %NAME%, %USERID%, %GROUP%. In place of {ROLE} put the desired role, example: %OVERWATCHHOURS%. You can also use for example %GLOBALHOURS%, %ALIVEHOURS%")]
        public string DiscordSummaryPerUserMessage { get; set; } = "";

        [Description("If set to true, will be sent without other messages piled up onto it as a part of maximizing messages while minimizing sent requests to Discord API. Recommended to leave as true to avoid potential cutting issues.")]
        public bool DiscordIsSummaryDedicated { get; set; } = true;
    }
}
