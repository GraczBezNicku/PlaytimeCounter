using HarmonyLib;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounterNWAPI.Patches
{
    [HarmonyPatch(typeof(Log), nameof(Log.Debug), new Type[] {typeof(string), typeof(string)})]
    public static class LogDebugPatch
    {
        public static bool Prefix(string message, string prefix = null)
        {
            if(Assembly.GetCallingAssembly() == Plugin.PlaytimeCounterAssembly)
            {
                if (Plugin.Instance.Config.DebugMode)
                {
                    Log.ConsoleWrite(Plugin.PlaytimeCounterAssembly, prefix, PluginAPI.Enums.LogType.Debug, message);
                }

                return false;
            }

            return true;
        }
    }
}
