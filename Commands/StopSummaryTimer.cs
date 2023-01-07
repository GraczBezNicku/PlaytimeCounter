using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PluginAPI.Core;

namespace PlaytimeCounterNWAPI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StopSummaryTimer : ICommand
    {
        public string Command { get; } = "StopSummaryTimer";

        public string[] Aliases { get; } = { "stopst" };

        public string Description { get; } = "Lets you disable automatic playtime checks.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            if(((CommandSender)sender).CheckPermission(PlayerPermissions.PermissionsManagement))
            {
                if(File.Exists(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt")))
                {
                    File.Delete(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt"));
                    response = "Summary timer stopped successfully.";
                }
                else
                {
                    response = "There is no summary timer active.";
                }
                return true;
            }
            else
            {
                response = "Insufficient permissions.";
                return false;
            }
        }
    }
}
