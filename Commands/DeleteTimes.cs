using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Commands
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
            if(playerSender.CheckPermission("pc.deletetimes"))
            {
                response = "Success!";
                DirectoryInfo di = new DirectoryInfo(Plugin.GroupDir);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                    playerSender.RemoteAdminMessage($"Removed file {file.Name}.");
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
