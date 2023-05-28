using MEC;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounterNWAPI
{
    public class EventsHandler
    {
        [PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
        public void OnWaitingForPlayers()
        {
            if (Plugin.Instance.coroutines.Count == 0)
            {
                Plugin.Instance.coroutines.Add(Timing.RunCoroutine(API.SendAfterCooldown()));
                Plugin.Instance.coroutines.Add(Timing.RunCoroutine(API.CheckForSummaryTime()));
            }
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerJoined)]
        public void OnJoin(Player p)
        {
            string pGroup = ServerStatic.PermissionsHandler._members.TryGetValue(p.UserId, out string groupName) ? groupName : null;

            if (pGroup == null)
                return;

            if (!Plugin.Instance.joinTime.ContainsKey(p.UserId) && Plugin.Instance.Config.groupsToLog.Contains(pGroup))
            {
                Plugin.Instance.joinTime.Add(p.UserId, DateTimeOffset.Now.ToUnixTimeSeconds());
            }
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLeft)]
        public void OnLeft(Player p)
        {
            if (Plugin.Instance.joinTime.ContainsKey(p.UserId))
            {
                long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds();
                long secondsPlayed = timeNow - Plugin.Instance.joinTime[p.UserId];
                if (!p.DoNotTrack)
                {
                    API.SendWebhook(p, secondsPlayed, API.UpdateType.Normal);
                    API.UpdateFiles(p, secondsPlayed / 60, API.UpdateType.Normal);
                }
                Plugin.Instance.joinTime.Remove(p.UserId);
            }
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerChangeRole)]
        public void OnRoleChange(Player player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason changeReason)
        {
            string pGroup = ServerStatic.PermissionsHandler._members.TryGetValue(player.UserId, out string groupName) ? groupName : null;

            if (pGroup == null)
                return;

            if (newRole == RoleTypeId.Overwatch)
            {
                if (!Plugin.Instance.beginOverwatchTime.ContainsKey(player.UserId) && Plugin.Instance.Config.groupsToLog.Contains(pGroup))
                    Plugin.Instance.beginOverwatchTime.Add(player.UserId, DateTimeOffset.Now.ToUnixTimeSeconds());
            }

            if(oldRole.RoleTypeId == RoleTypeId.Overwatch)
            {
                if(Plugin.Instance.beginOverwatchTime.ContainsKey(player.UserId))
                {
                    long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds();
                    long secondsPlayed = timeNow - Plugin.Instance.beginOverwatchTime[player.UserId];
                    if (!player.DoNotTrack)
                    {
                        API.SendWebhook(player, secondsPlayed, API.UpdateType.Overwatch);
                        API.UpdateFiles(player, secondsPlayed / 60, API.UpdateType.Overwatch);
                    }
                    Plugin.Instance.beginOverwatchTime.Remove(player.UserId);
                }
            }
        }
    }
}
