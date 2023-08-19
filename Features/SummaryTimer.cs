using Discord;
using GameCore;
using MEC;
using PlayerRoles;
using PlaytimeCounter.Enums;
using PlaytimeCounter.Features.Discord;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features
{
    public static class SummaryTimer
    {
        public static CoroutineHandle summaryHandle;

        public static IEnumerator<float> SummaryTimerCheck()
        {
            Helpers.LogDebug($"Summary coroutine running!");
            while(true)
            {
                yield return Timing.WaitForSeconds(Plugin.Instance.Config.DiscordWebhookCooldown);
                foreach(TrackingGroup group in TrackingGroup.TrackingGroups)
                {
                    if(DateTimeOffset.Now.ToUnixTimeSeconds() >= group.Config.SummaryTimerConfig.NextCheck)
                    {
                        PrepareSummary(group, group.Config.SummaryTimerConfig.RemoveTimes, true);
                    }
                }
            }
        }

        public static List<TrackedUser> SortListBySortingRule(TrackingGroup group)
        {
            List<TrackedUser> listToReturn = new List<TrackedUser>();

            switch (group.Config.SummaryTimerConfig.SortingType)
            {
                case SortingType.Time:
                    switch (group.Config.SummaryTimerConfig.TimeSortingRole)
                    {
                        case "Global": listToReturn = group.trackedUsers.OrderByDescending(x => x.GlobalTime).ToList(); break;
                        case "Alive": listToReturn = group.trackedUsers.OrderByDescending(x => x.AliveTime).ToList(); break;
                        default:
                            if (Enum.TryParse(group.Config.SummaryTimerConfig.TimeSortingRole, true, out RoleTypeId role))
                            {
                                listToReturn = group.trackedUsers.OrderByDescending(x => x.TimeTable[role]).ToList();
                            }
                            else
                            {
                                PluginAPI.Core.Log.Error($"Could not find RoleTypeId {group.Config.SummaryTimerConfig.TimeSortingRole}! Defaulting to global...");
                                listToReturn = group.trackedUsers.OrderByDescending(x => x.GlobalTime).ToList();
                            }
                            break;
                    }
                    break;
                case SortingType.Group: listToReturn = group.trackedUsers.OrderBy(x => x.Group).ToList(); break;
                case SortingType.Nickname: listToReturn = group.trackedUsers.OrderBy(x => x.Nickname).ToList(); break;
            }

            return listToReturn;
        }

        public static void PrepareSummary(TrackingGroup group, bool deleteUsers, bool affectNextCheck)
        {
            Helpers.LogDebug($"Creating summary for {group.Name} with deleteUsers set to {deleteUsers}");
            List<TrackedUser> usersToList = new List<TrackedUser>();
            List<TrackedUser> sortedUserList = SortListBySortingRule(group);

            if(group.CountingType == CountingType.User && sortedUserList.Count() < group.idsToLog.Count())
            {
                List<string> missingIds = group.idsToLog.Where(x => !sortedUserList.Any(y => y.UserId == x)).ToList();
                foreach(string id in missingIds)
                {
                    TrackedUser missingUser = new TrackedUser();

                    missingUser.TrackingGroup = group.Name;
                    missingUser.Nickname = "undefined";
                    missingUser.UserId = id;
                    missingUser.Group = "undefined";
                    missingUser.DntEnabled = false;
                    missingUser.GlobalTime = 0;
                    missingUser.AliveTime = 0;
                    missingUser.TimeTable = new Dictionary<RoleTypeId, long>();

                    foreach (RoleTypeId role in Enum.GetValues(typeof(RoleTypeId)))
                    {
                        missingUser.TimeTable.Add(role, 0);
                    }

                    sortedUserList.Add(missingUser);
                }
            }

            if(sortedUserList.Count() > group.Config.SummaryTimerConfig.MaxEntries) 
            {
                sortedUserList.RemoveRange(group.Config.SummaryTimerConfig.MaxEntries, sortedUserList.Count() - group.Config.SummaryTimerConfig.MaxEntries);
            }

            List<Discord.DiscordWebhook> webhooksToBeBundled = new List<Discord.DiscordWebhook>();

            Discord.DiscordWebhook summaryFirstWebhook = new Discord.SummaryFirstLineWebhook(group.Config.DiscordConfig.DiscordSummaryFirstMessage, 
                group.Config.DiscordConfig.DiscordWebhookURL, 
                group, 
                group.Config.SummaryTimerConfig.RoundTimeBetweenSummaries,
                DateTime.Now);
            webhooksToBeBundled.Add(summaryFirstWebhook);

            foreach(TrackedUser user in sortedUserList)
            {
                Discord.DiscordWebhook webhook = new Discord.SummaryUserWebhook(group.Config.DiscordConfig.DiscordSummaryPerUserMessage,
                    group.Config.DiscordConfig.DiscordWebhookURL,
                    group,
                    user);
                webhooksToBeBundled.Add(webhook);
            }

            DiscordWebhookBundle summaryBundle = new DiscordWebhookBundle(webhooksToBeBundled);
            Discord.DiscordWebhookHandler.WebhookBundleQueue.Enqueue(summaryBundle);

            if(deleteUsers)
            {
                Directory.Delete(group.trackedUsersDir, true);
                Directory.CreateDirectory(group.trackedUsersDir);
                group.trackedUsers.Clear();
                Helpers.LogDebug($"Deleted all user data for {group.Name}");
            }

            if(affectNextCheck)
            {
                group.Config.SummaryTimerConfig.RoundTimeBetweenSummaries = 0;
                group.Config.SummaryTimerConfig.NextCheck += group.Config.SummaryTimerConfig.CheckInterval;
                group.SaveConfig();
            }
        }
    }

    public class SummaryTimerConfig
    {
        public bool IsEnabled { get; set; } = false;

        [Description("Unix time when the next check should take place.")]
        public long NextCheck { get; set; }

        [Description("Time in seconds to add to NextCheck when a summary is printed.")]
        public long CheckInterval { get; set; }

        [Description("This variable holds round time in seconds to allow %ROUNDSECONDS%, %ROUNDMINUTES% and %ROUNDHOURS% to be used in summaries.")]
        public long RoundTimeBetweenSummaries { get; set; }

        [Description("Whether or not to remove times for a group when a summary is printed.")]
        public bool RemoveTimes { get; set; } = true;

        [Description("How many entries are to be printed out when a summary is made. (Will still remove all times if RemoveTimes is true!)")]
        public int MaxEntries { get; set; } = 10;

        [Description("Determines how to sort entries in the summary. (Group and Nickname alphabetically, Time descending)")]
        public SortingType SortingType { get; set; } = SortingType.Time;

        [Description("Only used when SortingType is set to 'Time'. Determines which role the list will be sorted by (use RoleTypeId, 'Alive' or 'Global')")]
        public string TimeSortingRole { get; set; } = "Global";
    }
}
