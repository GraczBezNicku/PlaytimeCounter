using CommandSystem;
using PlaytimeCounter.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RemoveAllTrackedUsers : ICommand
    {
        public string Command { get; } = "RemoveAllTrackedUsers";

        public string[] Aliases { get; } = { "ratr", };

        public string Description { get; } = "Removes all tracked users in a specified group.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission(PlayerPermissions.PermissionsManagement))
            {
                response = "Insufficient permissions.";
                return false;
            }

            if (arguments.Count() < 1)
            {
                response = "Wrong parameters! Usage: RemoveAllTrackedUsers <groupName>";
                return false;
            }

            if (!TrackingGroup.TrackingGroups.Any(x => x.Name == arguments.At(0)))
            {
                response = $"Group named {arguments.At(0)} does not exist!";
                return false;
            }

            TrackingGroup group = TrackingGroup.TrackingGroups.First(x => x.Name == arguments.At(0));

            Directory.Delete(group.trackedUsersDir, true);
            Directory.CreateDirectory(group.trackedUsersDir);
            response = "Success!";
            return true;
        }
    }
}
