using CommandSystem;
using PlaytimeCounter.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ForceSummary : ICommand
    {
        public string Command { get; } = "ForceSummary";

        public string[] Aliases { get; } = { "fsum", };

        public string Description { get; } = "Forces a summary for a specified group. Can specify if times are to be deleted and if NextCheck should be affected.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission(PlayerPermissions.PermissionsManagement))
            {
                response = "Insufficient permissions.";
                return false;
            }

            if(arguments.Count() < 3)
            {
                response = "Wrong parameters! Usage: ForceSummary <groupName> <deleteTimes> <nextCheckAffected>";
                return false;
            }

            if (!TrackingGroup.TrackingGroups.Any(x => x.Name == arguments.At(0)))
            {
                response = $"Group named {arguments.At(0)} does not exist!";
                return false;
            }

            TrackingGroup group = TrackingGroup.TrackingGroups.First(x => x.Name == arguments.At(0));
            bool deleteTimes = bool.Parse(arguments.At(1));
            bool nextCheckAffected = bool.Parse(arguments.At(2));

            SummaryTimer.PrepareSummary(group, deleteTimes, nextCheckAffected);
            response = "Success!";
            return true;
        }
    }
}
