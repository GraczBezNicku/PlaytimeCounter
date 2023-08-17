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
    public class RemoveTrackedUser : ICommand
    {
        public string Command { get; } = "RemoveTrackedUser";

        public string[] Aliases { get; } = { "rtu", };

        public string Description { get; } = "Removes a specified tracked user in a specified group.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission(PlayerPermissions.PermissionsManagement))
            {
                response = "Insufficient permissions.";
                return false;
            }

            if (arguments.Count() < 2)
            {
                response = "Wrong parameters! Usage: RemoveTrackedUser <groupName> <userid>";
                return false;
            }

            if (!TrackingGroup.TrackingGroups.Any(x => x.Name == arguments.At(0)))
            {
                response = $"Group named {arguments.At(0)} does not exist!";
                return false;
            }

            TrackingGroup group = TrackingGroup.TrackingGroups.First(x => x.Name == arguments.At(0));
            string userId = arguments.At(1);

            try
            {
                File.Delete(Path.Combine(group.trackedUsersDir, $"{userId}.yml"));
                response = "Success!";
                return true;
            }
            catch(Exception ex)
            {
                response = $"Could not remove the file! Exception: {ex.Message}";
                return false;
            }
        }
    }
}
