using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class Helpers
    {
        public static void LogDebug(string message)
        {
            if (Plugin.Instance.Config.DebugMode)
                Log.Debug(message);
        }
    }
}
