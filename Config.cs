using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("List of groups to log playtime of.")]
        public List<string> groupsToLog { get; set; } = new List<string>();

        public string webhookURL { get; set; } = "";

        [Description("Message that will display on discord. (can use {seconds} and {hours} too)")]
        public string webhookMessage { get; set; } = "{time} **{player}** left the server after playing for **{minutes}** minutes";
    }
}
