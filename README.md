# PlaytimeCounter
A very simple plugin that tracks playtime of specified groups on your SCP:SL EXILED Server.
# Reqs
EXILED 5.2.1
# Installation
1. Put PlaytimeCounter.dll in your Plugins folder.
2. Set up the config and start the server.
3. Done!
# Permissions and Commands
All are in Remote Admin

checktimes (ct) - `pc.checktimes` - lets you see playtime in saved files on your server.

deletetimes (dt) - `pc.deletetimes` - deletes all playtime files on your server storage.

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
