using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features.Discord
{
    public class DiscordWebhookBundle
    {
        public IEnumerable<DiscordWebhook> DiscordWebhooks;

        public DiscordWebhookBundle(IEnumerable<DiscordWebhook> webhooks, bool isDedicated)
        {
            DiscordWebhooks = webhooks;
        }
    }

    public class DiscordWebhook
    {
        public string TargetURL;
        public string Message;
        public TrackingGroup RequestingGroup;
        public Dictionary<string, object> SupportedDynamicValues;

        public DiscordWebhook(string message, string url, TrackingGroup requestingGroup) 
        {
            TargetURL = url;
            Message = message;
            RequestingGroup = requestingGroup;
        }

        public string FormattedMessage()
        {
            string newMessage = Message;

            foreach(string key in SupportedDynamicValues.Keys)
            {
                newMessage = newMessage.Replace(key, SupportedDynamicValues[key].ToString());
            }

            return newMessage;
        }
    }

    public sealed class PlayerJoinedWebhook : DiscordWebhook
    {
        public DateTime CurrentTime;
        public string Name, UserId, Group;

        public PlayerJoinedWebhook(string message, string url, TrackingGroup requestingGroup,
            DateTime currentTime,
            string name,
            string userid,
            string group) : base(message, url, requestingGroup)
        {
            CurrentTime = currentTime;
            Name = name;
            UserId = userid;
            Group = group;

            SupportedDynamicValues = new Dictionary<string, object>()
            {
                {"%TIME%", CurrentTime },
                {"%NAME%", Name},
                {"%USERID%", UserId},
                {"%GROUP%", Group},
            };
        }
    }

    public sealed class PlayerLeftWebhook : DiscordWebhook
    {
        public DateTime CurrentTime;
        public long Hours, Minutes, Seconds;
        public string Name, UserId, Group;

        public PlayerLeftWebhook(string message, string url, TrackingGroup requestingGroup,
            DateTime currentTime,
            long seconds,
            string name,
            string userid,
            string group) : base(message, url, requestingGroup)
        {
            CurrentTime = currentTime;
            Seconds = seconds;
            Minutes = Seconds / 60;
            Hours = Minutes / 60;
            Name = name;
            UserId = userid;
            Group = group;

            SupportedDynamicValues = new Dictionary<string, object>()
            {
                {"%TIME%", CurrentTime },
                {"%HOURS%", Hours},
                {"%MINUTES%", Minutes },
                {"%SECONDS%", Seconds},
                {"%NAME%", Name},
                {"%USERID%", UserId},
                {"%GROUP%", Group},
            };
        }
    }

    public sealed class PlayerChangedRoleToWebhook : DiscordWebhook
    {
        public DateTime CurrentTime;
        public string Name, UserId, Group;
        public RoleTypeId OldRole, NewRole;

        public PlayerChangedRoleToWebhook(string message, string url, TrackingGroup requestingGroup,
            DateTime currentTime,
            string name,
            string userid,
            string group,
            RoleTypeId oldRole,
            RoleTypeId newRole) : base(message, url, requestingGroup)
        {
            CurrentTime = currentTime;
            Name = name;
            UserId = userid;
            Group = group;
            OldRole = oldRole;
            NewRole = newRole;

            SupportedDynamicValues = new Dictionary<string, object>()
            {
                {"%TIME%", CurrentTime },
                {"%NAME%", Name},
                {"%USERID%", UserId},
                {"%GROUP%", Group},
                {"%OLDROLE%", OldRole },
                {"%NEWROLE%", NewRole},
            };
        }
    }

    public sealed class PlayerChangedRoleFromWebhook : DiscordWebhook
    {
        public DateTime CurrentTime;
        public string Name, UserId, Group;
        public long Seconds, Minutes, Hours;
        public RoleTypeId OldRole, NewRole;

        public PlayerChangedRoleFromWebhook(string message, string url, TrackingGroup requestingGroup,
            DateTime currentTime,
            string name,
            string userid,
            string group,
            long seconds,
            RoleTypeId oldRole,
            RoleTypeId newRole) : base(message, url, requestingGroup)
        {
            CurrentTime = currentTime;
            Name = name;
            UserId = userid;
            Group = group;
            Seconds = seconds;
            Minutes = Seconds / 60;
            Hours = Minutes / 60;
            OldRole = oldRole;
            NewRole = newRole;

            SupportedDynamicValues = new Dictionary<string, object>()
            {
                {"%TIME%", CurrentTime },
                {"%NAME%", Name},
                {"%USERID%", UserId},
                {"%GROUP%", Group},
                {"%SECONDS%", Seconds },
                {"%MINUTES%", Minutes },
                {"%HOURS%", Hours },
                {"%OLDROLE%", OldRole },
                {"%NEWROLE%", NewRole},
            };
        }
    }
}
