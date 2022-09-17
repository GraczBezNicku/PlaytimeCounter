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
    public class StartSummaryTimer : ICommand
    {
        public string Command { get; } = "StartSummaryTimer";

        public string[] Aliases { get; } = { "sst" };

        public string Description { get; } = "Lets you enable automatic playtime checks.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            if (playerSender.CheckPermission("pc.deletetimes"))
            {
                string[] argumentsArray = arguments.ToArray();

                string timeArg = argumentsArray[0];
                char lastLetter = timeArg[timeArg.Length - 1];
                string timeArgWithoutLast = timeArg.Remove(timeArg.Length - 1, 1);

                long time;

                switch(lastLetter)
                {
                    case 'm': time = long.Parse(timeArgWithoutLast) * 60; break;
                    case 'h': time = long.Parse(timeArgWithoutLast) * 3600; break;
                    case 'd': time = long.Parse(timeArgWithoutLast) * 86400; break;
                    case 'y': time = long.Parse(timeArgWithoutLast) * 31536000; break;
                    default: time = long.Parse(timeArg); break;
                }

                if(File.Exists(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt")))
                    File.Delete(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt"));

                using (StreamWriter sw = File.CreateText(Path.Combine(Plugin.GroupDir, "SummaryTimeCheck.txt")))
                {
                    sw.WriteLine($"{DateTimeOffset.Now.ToUnixTimeSeconds()}");
                    sw.WriteLine($"{time}");
                }

                response = "Succesfully created summary timer.";
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
