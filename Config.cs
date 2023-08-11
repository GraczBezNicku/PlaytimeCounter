using PlaytimeCounter.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class Config
    {
        public bool IsEnabled { get; set; }
        public bool DebugMode { get; set; }

        [Description("Lists all tracking groups. If a group with this name doesn't exist, it will automatically be created.")]
        public List<TrackingGroup> TrackingGroups { get; set; }
    }
}
