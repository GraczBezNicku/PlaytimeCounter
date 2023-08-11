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
    }
}
