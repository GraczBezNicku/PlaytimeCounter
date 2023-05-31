using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Microsoft.SqlServer.Server;
using PluginAPI.Core;

namespace PlaytimeCounterNWAPI.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CheckTime : ICommand
    {
        public string Command { get; } = "checktimes";

        public string[] Aliases { get; } = { "ct"};

        public string Description { get; } = "Lets you see staff's playtime.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission(PlayerPermissions.PlayerSensitiveDataAccess))
            {
                response = "Insufficient permissions.";
                return false;
            }

            response = "Success!\n";
            foreach (string fileName in Directory.GetFiles(Plugin.GroupDir))
            {
                Log.Debug($"Checking for file {fileName}");

                if (Path.GetFileName(fileName) == "SummaryTimeCheck.txt")
                    continue;

                string[] lines = File.ReadAllLines(Path.Combine(Plugin.GroupDir, fileName));

                if(lines.Length != 4)
                {
                    response += $"FILE FORMATTING FAILED ({Path.GetFileName(fileName)}): [";
                    foreach (string line in lines)
                        response += $"{line}, ";
                    response += "]\n";
                    continue;
                }

                response += $"{Path.GetFileName(fileName).Substring(0, Path.GetFileName(fileName).Length - 4)} ({lines[2]}): { lines[0]} (OV: {lines[1]})\n";           
            }
            return true;
        }
    }
}
