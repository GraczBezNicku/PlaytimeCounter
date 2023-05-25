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
    public class ForceSummary : ICommand
    {
        public string Command { get; } = "ForceSummary";

        public string[] Aliases { get; } = { "fsum" };

        public string Description { get; } = "Lets you force a playtime summary.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            bool deleteTimes;
            if (((CommandSender)sender).CheckPermission(PlayerPermissions.PermissionsManagement))
            {
                if (arguments.Count() < 1)
                    deleteTimes = true;
                else
                    deleteTimes = bool.Parse(arguments.At(0));

                API.PrepareSummary();

                if(deleteTimes)
                {
                    DirectoryInfo di = new DirectoryInfo(Plugin.GroupDir);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                        playerSender.ReferenceHub.queryProcessor._sender.RaReply($"PlaytimeCounter#Removed file {file.Name}.", true, true, string.Empty);
                    }

                    foreach (DirectoryInfo dir in di.GetDirectories())
                        dir.Delete(true);

                }

                response = "Success!";
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
