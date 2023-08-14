using PlaytimeCounter.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features
{
    public class SummaryTimer
    {
        //make it so when a summary is made with counting type "user" it will grab all ids and try to read their info. (0 or undefined if not found)
    }

    public class SummaryTimerConfig
    {
        public bool IsEnabled { get; set; }

        [Description("Unix time when the next check should take place.")]
        public ulong NextCheck { get; set; }

        [Description("Time in seconds to add to NextCheck when a summary is printed.")]
        public ulong CheckInterval { get; set; }

        [Description("Whether or not to remove times for a group when a summary is printed.")]
        public bool RemoveTimes { get; set; } = true;

        [Description("How many entries are to be printed out when a summary is made. (Will still remove all times if RemoveTimes is true!)")]
        public int MaxEntries { get; set; } = 10;

        [Description("Determines how to sort entries in the summary. (Group and Nickname alphabetically, Time descending)")]
        public SortingType SortingType { get; set; } = SortingType.Time;
    }
}
