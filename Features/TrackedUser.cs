using PlayerRoles;
using PluginAPI.Core;
using Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features
{
    public class TrackedUser
    {
        public string TrackingGroup { get; set; }
        public string Nickname { get; set; }
        public string UserId { get; set; }
        public string Group { get; set; }
        public bool DntEnabled { get; set; }
        public long GlobalTime { get; set; }
        public long AliveTime { get; set; }
        public Dictionary<RoleTypeId, long> TimeTable { get; set; }

        public static TrackedUser CreateTrackedUser(Player p, TrackingGroup trackingGroup)
        {
            TrackedUser user = new TrackedUser();

            user.TrackingGroup = trackingGroup.Name;
            user.Nickname = p.Nickname;
            user.UserId = p.UserId;
            user.Group = p.ReferenceHub.serverRoles.Group == null ? "default" : p.ReferenceHub.serverRoles.Group.GetGroupKey();
            user.DntEnabled = p.DoNotTrack;
            user.GlobalTime = 0;
            user.AliveTime = 0;
            user.TimeTable = new Dictionary<RoleTypeId, long>();
            
            foreach(RoleTypeId role in Enum.GetValues(typeof(RoleTypeId)))
            {
                user.TimeTable.Add(role, 0);
            }

            trackingGroup.trackedUsers.Add(user);

            SaveTrackedUser(user, trackingGroup);

            return user;
        }

        public static void SaveTrackedUser(TrackedUser user, TrackingGroup trackingGroup)
        {
            try
            {
                File.WriteAllText(Path.Combine(trackingGroup.trackedUsersDir, $"{user.UserId}.yml"), YamlParser.Serializer.Serialize(user));
            }
            catch(Exception ex)
            {
                Log.Error($"Failed saving a TrackedUser! Exception: {ex.Message}");
                return;
            }
        }

        public static bool TryGetTrackedUser(string userid, TrackingGroup trackingGroup, out TrackedUser user)
        {
            try
            {
                user = trackingGroup.trackedUsers.First(x => x.UserId == userid);

                if (user == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                user = null;
                return false;
            }
        }

        public static bool TryGetTrackedUser(Player p, TrackingGroup trackingGroup, out TrackedUser user)
        {
            try
            {
                user = trackingGroup.trackedUsers.First(x => x.UserId == p.UserId);

                if (user == null)
                    return false;
                else
                    return true;
            }
            catch(Exception ex)
            {
                user = null;
                return false;
            }
        }
    }
}
