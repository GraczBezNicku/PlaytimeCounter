using LabApi.Features.Console;

namespace PlaytimeCounter
{
    public class Helpers
    {
        public static void LogDebug(string message)
        {
            if (Plugin.Instance.Config.DebugMode)
                Logger.Debug(message);
        }
    }
}
