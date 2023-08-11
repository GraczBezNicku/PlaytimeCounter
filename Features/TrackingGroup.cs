using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaytimeCounter.Enums;
using Serialization;

namespace PlaytimeCounter.Features
{
    public class TrackingGroup
    {
        public List<UserGroup> GroupsToLog;

    }

    public class TrackingGroupConfig
    {
        [Description("Determines if individual users should be tracked instead of groups.")]
        public CountingType CountingType { get; set; }

        [Description("List of Groups / UserIDs of people that are to be tracked. Whether you should put UserIDs in there instead of groups relies on the CountingType config.")]
        public List<string> TrackingTargets { get; set; }
    }
}
