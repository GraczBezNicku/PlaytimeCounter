using Exiled.Events.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class EventsHandler
    {
        public void OnVerified(VerifiedEventArgs ev)
        {
            if(!Plugin.instance.joinTime.ContainsKey(ev.Player.RawUserId) && Plugin.instance.Config.groupsToLog.Contains(ev.Player.GroupName))
            {
                Plugin.instance.joinTime.Add(ev.Player.RawUserId, DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }

        public void OnDestroyed(DestroyingEventArgs ev)
        {
            if(Plugin.instance.joinTime.ContainsKey(ev.Player.RawUserId))
            {
                long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds();
                long secondsPlayed = timeNow - Plugin.instance.joinTime[ev.Player.RawUserId];
                if(!ev.Player.DoNotTrack)
                {
                    API.SendWebhook(ev.Player, secondsPlayed);
                    API.UpdateFiles(ev.Player, secondsPlayed / 60);
                }
                Plugin.instance.joinTime.Remove(ev.Player.RawUserId);
            }
        }
    }
}
