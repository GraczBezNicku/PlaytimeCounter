using CommandSystem;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounterNWAPI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DeleteTimes : ICommand
    {
        public string Command { get; } = "DeleteTimes";

        public string[] Aliases { get; } = { "dt" };

        public string Description { get; } = "Lets you delete staff's playtime history.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            if(((CommandSender)sender).CheckPermission(PlayerPermissions.PermissionsManagement))
            {
                response = "Success!";
                DirectoryInfo di = new DirectoryInfo(Plugin.GroupDir);

                foreach (FileInfo file in di.GetFiles())
                {
                    if (file.Name == "SummaryTimeCheck.txt")
                        continue;

                    file.Delete();
                    playerSender.ReferenceHub.queryProcessor._sender.RaReply($"PlaytimeCounter#Removed file {file.Name}.", true, true, string.Empty);
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);

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
