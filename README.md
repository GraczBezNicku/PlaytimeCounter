# PlaytimeCounter
A very simple plugin that tracks playtime of specified groups on your SCP:SL Server.
# Installation
1. Put PlaytimeCounter.dll in your Plugins folder.
2. Set up the config and start the server.
3. Done!
# Permissions and Commands
All are in Remote Admin

`checktimes` (`ct`) - `PlayerSensitiveDataAccess` - lets you see playtime in saved files on your server.

`deletetimes` (`dt`) - `PermissionsManagement` - deletes all playtime files on your server storage.

`startsummarytimer <check_interval (exapmles: 1m, 1h, 1d, 1y)>` (`sst`) - `PermissionsManagement` - Sets up a timer that automatically counts total playtime of your staff members

`stopsummarytimer` (`stopst`) - `PermissionsManagement` - Deletes the timer if it exists.

`forcesummary <delete_times?bool>` (`fsum`) - `PermissionsManagement` - Forces a summary printout on discord.

# Default Config
```
is_enabled: true
debug_mode: true
# This allows me to see how many people actually use this plugin. It will only inform me of the following: Server Name, Server IP & Port, Time at which your server started.
server_tracking: true
# List of groups to log playtime of.
groups_to_log: []
# List of groups and their playtime requirement in seconds. If met, reqResult will change based on your config.
group_reqs:
  owner: 12600
# List of groups and their playtime requirement (in overwatch mode) in seconds. If met, reqResultOv will change based on your config.
group_reqs_ov:
  owner: 12600
webhook_u_r_l: ''
webhook_avatar_u_r_l: https://cdn.discordapp.com/attachments/434037173281488899/940610688760545290/mrozonyhyperthink.jpg
discord_webhook_cooldown: 10
summary_check_cooldown: 300
# First line when printing playtime summary
webhook_count_message: >
  Playtime summary: 
# Lines printed out for users which have playtime recorded. Accepts parameters: {steamID64}, {nickname}, {group}, {hours}, {minutes}, {seconds}, {hoursOv}, {minutesOv}, {secondsOv}, {reqResult}, {reqResultOv}
webhook_count_user_message: >
  **{nickname} ({steamID64}) [{group}]** has played for **{minutes}m ({minutesOv})** - Normal criteria met? {reqResult}, Overwatch criteria met? {reqResultOv} 
webhook_req_result_met: ':white_check_mark:'
webhook_req_result_not_met: ':x:'
webhook_req_result_unknown: ':warning:'
# Message that will display on discord when a tracked player leaves the server. (can use {seconds} and {hours} too)
webhook_message: '{time} **{player}** left the server after playing for **{minutes}** minutes'
# Message that will display on discord when a tracked player leaves Overwatch mode. (can use {seconds} and {hours} too)
webhook_message_overwatch: '{time} **{player}** left Overwatch mode after playing for **{minutes}** minutes'
```
