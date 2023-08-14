using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter.Features
{
    public class TrackedUser
    {
        public string DisplayNickname { get; set; }
        public bool DntEnabled { get; set; }
        public string Group { get; set; }
        public Dictionary<RoleTypeId, ulong> TimeTable { get; set; }
    }
}
