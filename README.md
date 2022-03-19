# PlaytimeCounter
A very simple plugin that tracks playtime of specified groups on your SCP:SL EXILED Server.
# Reqs
A discord channel with a webhook, and at least EXILED 4.2.5
# Installation
1. Put PlaytimeCounter.dll in your Plugins folder.
2. Put Newtonsoft.Json.dll in your Plugins -> dependencies folder.
3. Set up the config and start the server.
4. Done!
# Important Info
This plugin puts faith in your staff members that they won't have any issues tracking their time, as this plugin does not check for DNT status.
# Default Config
```
playtime_counter:
  is_enabled: true
  # List of groups to log playtime of.
  groups_to_log: []
  webhook_u_r_l: ''
  # Message that will display on discord. (can use {seconds} and {hours} too)
  webhook_message: '{time} {player} left the server after playing for {minutes} minutes'
```
