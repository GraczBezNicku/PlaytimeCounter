using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features
{
    public class SummaryTimer
    {

    }

    public class SummaryTimerConfig
    {
        public bool IsEnabled { get; set; }
        public ulong NextCheck { get; set; }
        public ulong CheckInterval { get; set; }
        public bool RemoveTimes { get; set; }
    }
}
