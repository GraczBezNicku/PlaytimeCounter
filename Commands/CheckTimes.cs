using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using PlayerRoles;
using PlaytimeCounter.Features;

namespace PlaytimeCounter.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CheckTimes : ICommand
    {
        public string Command { get; } = "CheckTimes";

        public string[] Aliases { get; } = { "ct", };

        public string Description { get; } = "Shows current tracked time for a specified group.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission(PlayerPermissions.PlayerSensitiveDataAccess))
            {
                response = "Insufficient permissions.";
                return false;
            }

            if (arguments.Count() < 1)
            {
                response = "Wrong parameters! Usage: CheckTimes <groupName>";
                return false;
            }

            if(!TrackingGroup.TrackingGroups.Any(x => x.Name == arguments.At(0)))
            {
                response = $"Group named {arguments.At(0)} does not exist!";
                return false;
            }

            TrackingGroup group = TrackingGroup.TrackingGroups.First(x => x.Name == arguments.At(0));
            List<TrackedUser> sortedUserList = SummaryTimer.SortListBySortingRule(group);

            if (sortedUserList.Count() > group.Config.SummaryTimerConfig.MaxEntries)
            {
                sortedUserList.RemoveRange(group.Config.SummaryTimerConfig.MaxEntries, sortedUserList.Count() - group.Config.SummaryTimerConfig.MaxEntries);
            }

            response = "Success!\n";

            foreach(TrackedUser user in sortedUserList)
            {
                response += $"{user.Nickname} ({user.UserId}) [{user.Group}]:\nGlobalTime: {user.GlobalTime}\nAliveTime: {user.AliveTime}\n";
                foreach(RoleTypeId key in user.TimeTable.Keys)
                {
                    response += $"{key}: {user.TimeTable[key]}\n";
                }
            }

            return true;
        }
    }
}
