# PlaytimeCounter
This is a plugin designed to track playtime of your staff members and players. 
This exists for people that may want time tracking but don't want all the other features that come with other plugins.

# features
1. Playtime Counting.
2. Functionality of multiple tracking groups, allowing for different settings.
3. Automatic summaries to keep track of your staff members.
4. Discord Integration.

# Installation
1. Download the .DLL Plugin file and Dependencies.zip
2. Throw the plugin .DLL file in PluginAPI/plugins/*port*
3. Unpack all Dependencies into PluginAPI/plugins/*port*/dependencies
4. Set up config.yml and launch the server!

# Example config
By default, PlaytimeCounter will generate a sample config so you can wrap your head around the config system, which has dramatically changed since the last PlaytimeCounter release.
In the ExampleConfig folder you can find the default configuration PlaytimeCounter generates.

# Permissions and Commands
All commands are executed in Remote Admin:

`CheckTimes <groupName>` (`ct`) - `PlayerSensitiveDataAccess` - Will print out the playtime data for a specified group.

`ForceSummary <groupName> <deleteTimes(bool)> <affectNextCheck(bool)>` (`fsum`) - `PermissionsManagement` - Will force a discord summary. AffectNextCheck determines whether or not the next summary will be postponed by the CheckInterval value.

`RemoveTrackedUser <groupName> <userId>` (`rtu`) - `PermissionsManagement` - Will remove a tracked user by their userId from a specified group.

`RemoveAllTrackedUsers <groupName>` (`ratu`) - `PermissionsManagement` - Will remove all tracked users from a specified group.