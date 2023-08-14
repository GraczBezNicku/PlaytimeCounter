using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features.Discord
{
    public class DiscordWebhookBundle
    {
        public bool IsDedicated;
        public IEnumerable<DiscordWebhook> DiscordWebhooks;

        public DiscordWebhookBundle(IEnumerable<DiscordWebhook> webhooks, bool isDedicated)
        {
            IsDedicated = isDedicated;
            DiscordWebhooks = webhooks;
        }
    }

    public class DiscordWebhook
    {
        public string TargetURL;
        public string Message;
        public Dictionary<string, object> SupportedDynamicValues;

        public DiscordWebhook(string message, string url) 
        {
            TargetURL = url;
            Message = message;
        }
    }

    public sealed class PlayerJoinedWebhook : DiscordWebhook
    {
        public DateTime CurrentTime;
        public string Name, UserId, Group;

        public PlayerJoinedWebhook(string message, string url,
            DateTime currentTime,
            string name,
            string userid,
            string group) : base(message, url)
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
        public int Hours, Minutes, Seconds;
        public string Name, UserId, Group;

        public PlayerLeftWebhook(string message, string url,
            DateTime currentTime,
            int hours,
            int minutes,
            int seconds,
            string name,
            string userid,
            string group) : base(message, url)
        {
            CurrentTime = currentTime;
            hours = Hours;
            Minutes = minutes;
            Seconds = seconds;
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

        public PlayerChangedRoleToWebhook(string message, string url,
            DateTime currentTime,
            string name,
            string userid,
            string group) : base(message, url)
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

    public sealed class PlayerChangedRoleFromWebhook : DiscordWebhook
    {
        public DateTime CurrentTime;
        public string Name, UserId, Group;

        public PlayerChangedRoleFromWebhook(string message, string url,
            DateTime currentTime,
            string name,
            string userid,
            string group) : base(message, url)
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
}
