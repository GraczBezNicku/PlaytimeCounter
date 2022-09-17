using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Permissions.Extensions;
using System.IO;

namespace PlaytimeCounter.Commands
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
            if(playerSender.CheckPermission("pc.deletetimes"))
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
