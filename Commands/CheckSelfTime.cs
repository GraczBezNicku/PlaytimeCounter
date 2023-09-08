using CommandSystem;
using PlayerRoles;
using PlaytimeCounter.Features;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Experimental.Rendering;

namespace PlaytimeCounter.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class CheckSelfTime : ICommand
    {
        public string Command { get; } = "CheckSelfTime";

        public string[] Aliases { get; } = { "checkst" };

        public string Description { get; } = "Shows player's playtime for all groups.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player pSender = Player.Get(sender);

            if(!TrackingGroup.TrackingGroups.Any(x => x.trackedUsers.Any(x => x.UserId == pSender.UserId) && x.Config.AppearInSelfCheck))
            {
                response = "You are not tracked by any group / The group you are being tracked by does not support self checking.";
                return false;
            }

            List<TrackedUser> senderInstances = new List<TrackedUser>();

            foreach(TrackingGroup group in TrackingGroup.TrackingGroups)
            {
                if (!group.Config.AppearInSelfCheck)
                    continue;

                if (!group.trackedUsers.Any(x => x.UserId == pSender.UserId))
                    continue;

                senderInstances.Add(group.trackedUsers.First(x => x.UserId == pSender.UserId));
            }

            response = "Success!\n";

            foreach (TrackedUser user in senderInstances)
            {
                response += $"{user.Nickname} ({user.UserId}) [{user.Group}]:\nGlobalTime: {user.GlobalTime}\nAliveTime: {user.AliveTime}\n";
                foreach (RoleTypeId key in user.TimeTable.Keys)
                {
                    response += $"{key}: {user.TimeTable[key]}\n";
                }
            }

            return true;
        }
    }
}
