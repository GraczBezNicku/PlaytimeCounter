using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace PlaytimeCounter.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CheckTime : ICommand
    {
        public string Command { get; } = "checktimes";

        public string[] Aliases { get; } = { "ct"};

        public string Description { get; } = "Lets you see staff's playtime.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("pc.checktimes"))
            {
                response = "Insufficient permissions.";
                return false;
            }

            response = "Success!\n";
            foreach (string fileName in Directory.GetFiles(Plugin.GroupDir))
            {
                string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, fileName));
                response += $"{Path.GetFileName(fileName).Substring(0, Path.GetFileName(fileName).Length - 4)}: { lines[0]}\n";           
            }
            return true;
        }
    }
}
